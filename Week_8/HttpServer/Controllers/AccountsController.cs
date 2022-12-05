using System.Text;
using System.Text.Json;
using HttpServer.Attributes;
using HttpServer.MyORM;
using Npgsql;

namespace HttpServer.Controllers;

[HttpController("accounts")]
internal class AccountsController
{
    private static string _connectionStr = "Server=localhost;Database=postgres;Port=5432;SSLMode=Prefer";

    // static byte[] dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
    // private DBSettings _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

    // private static string Host = "localhost";
    // private static string User = "root";
    // private static string DBname = "postgres";
    // private static string Password = "1234";
    // private static string Port = "5432";

    private static AccountDAO _accountDao = new AccountDAO(_connectionStr);

    [HttpGET("cool")]
    public string cool()
    {
        return "Good response";
    }

    [HttpGET("accounts/getAccounts")]
    public string getAccounts()
    {
        var accounts = _accountDao.GetAccounts();
        if (accounts is null) return "Accounts not found";
        var accountList = new StringBuilder();
        foreach (var user in accounts)
        {
            accountList.Append($"User with id = {user.Id}, login =  {user.Login}, password = {user.Password}   ");
        }

        return accountList.ToString();
    }


    [HttpGET("accounts/getAccountById")]
    public string getAccountById(int userId)
    {
        var account = _accountDao.GetAccountById(userId);

        if (account == null) return "User not found";
        return string.Format("User with id = {0}, login =  {1}, password = {2}",
            account.Id.ToString(),
            account.Login,
            account.Password);
    }

    [HttpPOST("accounts/saveAccount")]
    public string saveAccount(string query)
    {
        var queryParams = query.Split('&')
            .SelectMany(pair => pair.Split('='))
            .ToArray();
        var res = _accountDao.InsertAccount(new Account() { Login = queryParams[1], Password = queryParams[3] });
        if (res == 0)
        {
            Console.WriteLine("Error while saving data");
            return "Error while saving data";
        }

        Console.WriteLine("Data saved successfully");
        return "Data saved successfully";
    }
}