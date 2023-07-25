using TodoAppCore.Models;

namespace TodoAppCore.Services;

public class TodoService : ITodoService
{
    private readonly List<ToDo> _toDoList;

    public TodoService()
    {
        _toDoList = new List<ToDo>();
    }

    public ToDo Add(ToDo todo)
    {
        if (!IsValid(todo)) throw new Exception("ToDo Is Not Valid");

        todo.Id = Guid.NewGuid().ToString();
        todo.LastUpdateDate = DateTime.Now;
        _toDoList.Add(todo);
        return todo;
    }

    public ToDo Update(ToDo todo)
    {
        var index = _toDoList.FindIndex(x => x.Id == todo.Id);
        _toDoList[index] = todo;
        return todo;
    }

    public ToDo Delete(string id)
    {
        var todo = GetById(id);
        if (todo == null) throw new ArgumentNullException(nameof(todo));

        _toDoList.Remove(todo);
        return todo;
    }

    public ToDo? GetById(string id)
    {
        return _toDoList.Find(x => x.Id == id);
    }

    public List<ToDo> GetAll()
    {
        return _toDoList;
    }

    private bool IsValid(ToDo todo)
    {
        var toDoItem = _toDoList.FirstOrDefault(x => x.Title.ToLower() == todo.Title.ToLower());
        return toDoItem == null;
    }
}