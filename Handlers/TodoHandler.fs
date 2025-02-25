namespace Handlers

open Giraffe
open Microsoft.AspNetCore.Http
open Models
open System.Threading

module TodoHandler =

    let mutable todos = []
    let idCounter = ref 1

    let getTodos =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            json todos next ctx

    let getTodoById (id: int) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            match todos |> List.tryFind (fun t -> t.Id = id)  with
            | Some todo -> json todo next ctx
            | None -> RequestErrors.NOT_FOUND "Todo not found" next ctx

    let addTodo =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! newTodo = ctx.BindJsonAsync<Todo>()
                let todo = { newTodo with Id = Interlocked.Increment(idCounter) }
                todos <- todo :: todos
                return! json todo next ctx
            }

    let updateTodo (id: int) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! updatedTodo = ctx.BindJsonAsync<Todo>()
                match todos |> List.tryFindIndex (fun t -> t.Id = id) with
                | Some index ->
                    let updatedTodos = 
                        todos |> List.mapi (fun i t -> if i = index then { updatedTodo with Id = id } else t)
                    todos <- updatedTodos 
                    return! json updatedTodo next ctx

                | None ->
                    return! RequestErrors.NOT_FOUND "Todo not found" next ctx
            }

    let deleteTodo (id: int) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            match List.tryFind (fun t -> t.Id = id) todos with
            | Some _ ->
                todos <- List.filter (fun t -> t.Id <> id) todos  // ✅ Correctly removing from immutable list
                text "Deleted successfully" next ctx
            | None ->
                RequestErrors.NOT_FOUND "Todo not found" next ctx
