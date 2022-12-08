namespace HttpServer.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    int Create(T entity);
    int Update(T entity);
    int Delete(T entity);
    int Delete(int id);
}