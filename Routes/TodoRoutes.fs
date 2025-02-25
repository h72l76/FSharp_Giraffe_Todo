namespace Routes

open Giraffe
open Handlers.TodoHandler

module TodoRoutes =
    let routes : HttpHandler=
        choose [
            route "/todos" >=> choose [
                GET >=> getTodos
                POST >=> addTodo
            ]
            routef "/todos/%i" (fun id -> choose [
                GET >=> getTodoById id
                PUT >=> updateTodo id
                DELETE >=> deleteTodo id
            ])
        ]
