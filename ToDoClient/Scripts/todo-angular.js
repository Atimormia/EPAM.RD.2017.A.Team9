function TodoController($scope, $http) {
    $scope.appTitle = "ToDo App";
    $scope.appHeadline = "Team9";
    $scope.saved = localStorage.getItem('todos');
    $scope.todos = (localStorage.getItem('todos') !== null) ? JSON.parse($scope.saved) : [{ text: 'Learn AngularJS', done: false }, { text: 'Build an Angular app', done: false }];
    localStorage.setItem('todos', JSON.stringify($scope.todos));

    $scope.addTodo = function () {
        $scope.todos.push({
            text: $scope.todoText,
            done: false
        });
        $scope.todoText = ''; //clear the input after adding
        localStorage.setItem('todos', JSON.stringify($scope.todos));

        var newItem = {
            IsCompleted: false,
            Name: $scope.todoText
        };
        //calling controller method
        $http({
            method: "PUT",
            url: "/api/todos",
            data: newItem
        });
        //tasksManager.createTask($scope.todo.done,$scope.todoText)
        //    .then(tasksManager.loadTasks);
    };

    $scope.remaining = function () {
        var count = 0;
        angular.forEach($scope.todos, function (todo) {
            count += todo.done ? 0 : 1;
        });
        return count;
    };

    $scope.archive = function () {
        var oldTodos = $scope.todos;
        $scope.todos = [];
        angular.forEach(oldTodos, function (todo) {
            if (!todo.done)
                $scope.todos.push(todo);
        });
        localStorage.setItem('todos', JSON.stringify($scope.todos));
    };
}