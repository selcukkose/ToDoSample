using TodoAppCore.Models;

namespace TodoAppCore.Services;

public interface ITodoService
{
    ToDo Add(ToDo todo);

    ToDo Update(ToDo todo);

    ToDo Delete(string id);

    ToDo? GetById(string id);

    List<ToDo> GetAll();
}