using System.Text;
using System.Text.Json;
using HttpServer.Attributes;
using Npgsql;

namespace HttpServer.Controllers;

[HttpController("accounts")]
internal class Accounts
{
    static byte[] dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
    private DBSettings _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

    private static string Host = "localhost";
    private static string User = "root";
    private static string DBname = "postgres";
    private static string Password = "1234";
    private static string Port = "5432";

    [HttpGET("cool")]
    public string cool()
    {
        return "Good response";
    }

    [HttpGET("accounts/getAccounts")]
    public string getAccounts()
    {
        var accounts = new List<Account>();
        string connString =
            String.Format(
                "Server={0};Database={1};Port={2};SSLMode=Prefer",
                Host,
                DBname,
                Port);

        using (var conn = new NpgsqlConnection(connString))
        {
            Console.Out.WriteLine("Opening connection");
            conn.Open();

            using (var command = new NpgsqlCommand($"SELECT * FROM public.\"accounts\"", conn))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var login = reader.GetString(1);
                    var password = reader.GetString(2);
                    accounts.Add(new Account() { Id = id, Login = login, Password = password });
                }

                reader.Close();
            }
        }

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
        var account = new Account();
        string connString =
            String.Format(
                "Server={0};Database={1};Port={2};SSLMode=Prefer",
                Host,
                DBname,
                Port);

        using (var conn = new NpgsqlConnection(connString))
        {
            Console.Out.WriteLine("Opening connection");
            conn.Open();

            using (var command =
                   new NpgsqlCommand($"SELECT * FROM public.\"accounts\" as a WHERE a.id = {userId}", conn))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var login = reader.GetString(1);
                    var password = reader.GetString(2);
                    account = new Account() { Id = id, Login = login, Password = password };
                }

                reader.Close();
            }
        }

        if (account.Id == 0) return "User not found";
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

        string connString =
            String.Format(
                "Server={0};Database={1};Port={2};SSLMode=Prefer",
                Host,
                DBname,
                Port);

        using (var conn = new NpgsqlConnection(connString))

        {
            Console.Out.WriteLine("Opening connection");
            conn.Open();

            using (var command =
                   new NpgsqlCommand("INSERT INTO public.\"accounts\" (login, password) VALUES (@l1, @p1)", conn))
            {
                command.Parameters.AddWithValue("l1", queryParams[1]);
                command.Parameters.AddWithValue("p1", queryParams[3]);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Data saved successfully");
        return "Data saved successfully";
    }
}