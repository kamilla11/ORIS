namespace HttpServer.MyORM;

public class AccountDAO
{
    private static string _connectionStr;
    private List<Account> accounts;

    public AccountDAO(string connectionString)
    {
        _connectionStr = connectionString;
        accounts = GetAccounts().ToList();
    }

    public Account GetAccountById(int id)
    {
        return new Database(_connectionStr).Select<Account>(id);
    }

    public IEnumerable<Account> GetAccounts()
    {
        return new Database(_connectionStr).Select<Account>();
    }

    public int InsertAccount(Account entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int DeleteAccount(int id)
    {
        return new Database(_connectionStr).Delete<Account>(id);
    }

    public int DeleteAccount(Account entity)
    {
        return new Database(_connectionStr).Delete<Account>(entity);
    }

    public int UpdateAccount<TParam>(Account entity)
    {
        return new Database(_connectionStr).Update<Account>(entity);
    }

    public bool VerifyLoginAndPassword(string login, string password)
    {
        var account = accounts.Find(a => a.Login == login && a.Password == password);
        return account is not null;
    }
}