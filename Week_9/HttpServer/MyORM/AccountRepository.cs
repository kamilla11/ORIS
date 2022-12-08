using HttpServer.Interfaces;

namespace HttpServer.MyORM;

public class AccountRepository : IRepository<Account>
{
    private static string _connectionStr;

    public AccountRepository(string connectionString)
    {
        _connectionStr = connectionString;
    }

    public Account GetById(int id)
    {
        return new Database(_connectionStr).Select<Account>(id);
    }

    public IEnumerable<Account> GetAll()
    {
        return new Database(_connectionStr).Select<Account>();
    }

    public int Create(Account entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int Update(Account entity)
    {
        return new Database(_connectionStr).Update<Account>(entity);
    }


    public int Delete(Account entity)
    {
        return new Database(_connectionStr).Delete<Account>(entity);
    }

    public int Delete(int id)
    {
        return new Database(_connectionStr).Delete<Account>(id);
    }
}