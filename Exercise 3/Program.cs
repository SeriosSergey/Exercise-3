using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_3
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Старт приложения-банкомата...");

            var atmManager = CreateATMManager();

            //Вывод данных пользователя по логину и паролю
            var user = atmManager.GetUser("snow", "111");
            Console.WriteLine($"{user.Id}. {user.SurName} {user.FirstName} {user.MiddleName} Phone:{user.Phone} Passport: {user.PassportSeriesAndNumber} Registration date: {user.RegistrationDate} Login: {user.Login} Password: {user.Password}");
            Console.WriteLine();

            //Вывод данных о всех счетах заданного пользователя
            var accounts = atmManager.GetUserAccounts(user);
            foreach(var account in accounts)
            {
                Console.WriteLine($"{account.Id}. Opening date: {account.OpeningDate} Cash all: {account.CashAll}");
            }
            Console.WriteLine();

            //Вывод данных о всех счетах заданного пользователя, включая историю по каждому счету
            var accounts_and_hist = atmManager.GetUsersAccountsAndHistory(user);
            foreach (var account in accounts_and_hist)
            {
                Console.WriteLine($"{account.Id}. Opening date: {account.OpeningDate} Cash all: {account.CashAll}");
                foreach (var hist in account.history)
                {
                    Console.WriteLine($"\t{hist.Id}. Operation date: {hist.OperationDate} Type: {hist.OperationType} Cash sum: {hist.CashSum}");
                }
            }
            Console.WriteLine();

            //Вывод данных о всех операциях пополенения счёта с указанием владельца каждого счёта;
            var account_replenishment_operations = atmManager.GetAccountReplenishmentOperations();
            foreach (var account in account_replenishment_operations)
            {
                Console.WriteLine($"{account.Id}. Operation date: {account.OperationDate} Type: {account.OperationType} Cash sum: {account.CashSum} User: {account.UserId}. {account.FirstName} {account.MiddleName} {account.SurName}");
            }
            Console.ReadLine();

            System.Console.WriteLine("Завершение работы приложения-банкомата...");
        }

        static ATMManager CreateATMManager()
        {
            var dataContext = new ATMDataContext();
            var users = dataContext.Users.ToList();
            var accounts = dataContext.Accounts.ToList();
            var history = dataContext.History.ToList();

            return new ATMManager(accounts, users, history);
        }

        public class Account
        {
            public int Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public decimal CashAll { get; set; }
            public int UserId { get; set; }
        }

        public class OperationsHistory
        {
            public int Id { get; set; }
            public DateTime OperationDate { get; set; }
            public OperationType OperationType { get; set; }
            public decimal CashSum { get; set; }
            public int AccountId { get; set; }
        }

        public enum OperationType
        {
            InputCash = 1,
            OutputCash = 2
        }

        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string SurName { get; set; }
            public string MiddleName { get; set; }
            public string Phone { get; set; }
            public string PassportSeriesAndNumber { get; set; }
            public DateTime RegistrationDate { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        public class ATMManager
        {
            public IEnumerable<Account> Accounts { get; private set; }

            public IEnumerable<User> Users { get; private set; }

            public IEnumerable<OperationsHistory> History { get; private set; }

            public ATMManager(IEnumerable<Account> accounts, IEnumerable<User> users, IEnumerable<OperationsHistory> history)
            {
                Accounts = accounts;
                Users = users;
                History = history;
            }

            public User GetUser(string Login, string Password)
            {
                try
                {
                    var user = Users.First(u => u.Login == Login && u.Password == Password);
                    return user;
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<Account> GetUserAccounts(User user)
            {
                var accounts = Accounts.Where(a => a.UserId == user.Id).ToList();
                return accounts;
            }

            public dynamic GetUsersAccountsAndHistory(User user)
            {
                var result = Accounts.Select(a => new {
                        a.Id,
                        a.OpeningDate,
                        a.CashAll,
                        a.UserId,
                        history = History.Where(h=>h.AccountId==a.Id)
                    })
                    .Where(a=>a.UserId==user.Id)
                    .ToList();
                return result;
            }

            public dynamic GetAccountReplenishmentOperations()
            {
                var result = History.Where(h => h.OperationType == OperationType.InputCash)
                    .Join(Accounts, h => h.AccountId, a => a.Id, (h, a) => new { h.Id, h.OperationDate, h.OperationType,h.CashSum, a.UserId })
                    .Join(Users, h => h.UserId, u => u.Id, (h, u) => new { h.Id, h.OperationDate, h.OperationType,h.CashSum, UserId = u.Id, u.FirstName, u.MiddleName, u.SurName })
                    .ToList();
                return result;
            }
        }

        public class ATMDataContext
        : IDisposable
        {
            public IQueryable<User> Users =>
                new List<User>()
                {
                new User{Id = 1, SurName = "Snow", FirstName = "Mike", MiddleName = "Bob", Phone = "111111", PassportSeriesAndNumber = "6666 123421", RegistrationDate = DateTime.Now.AddMonths(-12), Login = "snow", Password = "111" },
                new User{Id = 2, SurName = "Lee", FirstName = "Ann", MiddleName = "Mike", Phone = "222222", PassportSeriesAndNumber = "7777 123123", RegistrationDate = DateTime.Now.AddMonths(-22), Login = "lee", Password = "222" },
                new User{Id = 3, SurName = "Cant", FirstName = "Greg", MiddleName = "Tor", Phone = "333333", PassportSeriesAndNumber = "8888213124", RegistrationDate = DateTime.Now.AddMonths(-42), Login = "cant", Password = "333" },
                new User{Id = 4, SurName = "Star", FirstName = "Jonh", MiddleName = "Ctor", Phone = "444444", PassportSeriesAndNumber = "9999123123", RegistrationDate = DateTime.Now.AddMonths(-15), Login = "star", Password = "444" },
                new User{Id = 5, SurName = "Cop", FirstName = "Mike", MiddleName = "Prop", Phone = "555555", PassportSeriesAndNumber = "2133321414", RegistrationDate = DateTime.Now.AddMonths(-32), Login = "cop", Password = "555" }
                }.AsQueryable();

            public IQueryable<Account> Accounts =>
                new List<Account>()
                {
                new Account{Id = 1, OpeningDate = DateTime.Now.AddMonths(-12), CashAll = 100500m, UserId = 1 },
                new Account{Id = 2, OpeningDate = DateTime.Now.AddMonths(-10), CashAll = 500m, UserId = 1 },
                new Account{Id = 3, OpeningDate = DateTime.Now.AddMonths(-2), CashAll = 100m, UserId = 1 },
                new Account{Id = 4, OpeningDate = DateTime.Now.AddMonths(-22), CashAll = 10000m, UserId = 2 },
                new Account{Id = 5, OpeningDate = DateTime.Now.AddMonths(-12), CashAll = 5000m, UserId = 2 },
                new Account{Id = 6, OpeningDate = DateTime.Now.AddMonths(-42), CashAll = 10500m, UserId = 3 },
                new Account{Id = 7, OpeningDate = DateTime.Now.AddMonths(-15), CashAll = 1500m, UserId = 4 },
                new Account{Id = 8, OpeningDate = DateTime.Now.AddMonths(-32), CashAll = 10000m, UserId = 5 },
                new Account{Id = 9, OpeningDate = DateTime.Now.AddMonths(-3), CashAll = 100550m, UserId = 5 }
                }.AsQueryable();

            public IQueryable<OperationsHistory> History =>
                new List<OperationsHistory>()
                {
                new OperationsHistory{Id = 1, OperationDate = DateTime.Now.AddDays(-30), OperationType = OperationType.InputCash, CashSum = 100m, AccountId = 1 },
                new OperationsHistory{Id = 2, OperationDate = DateTime.Now.AddDays(-20), OperationType = OperationType.OutputCash, CashSum = 50m, AccountId = 1 },
                new OperationsHistory{Id = 3, OperationDate = DateTime.Now.AddDays(-10), OperationType = OperationType.InputCash, CashSum = 100m, AccountId = 1 },
                new OperationsHistory{Id = 4, OperationDate = DateTime.Now.AddDays(-15), OperationType = OperationType.InputCash, CashSum = 300m, AccountId = 2 },
                new OperationsHistory{Id = 5, OperationDate = DateTime.Now.AddDays(-5), OperationType = OperationType.OutputCash, CashSum = 100m, AccountId = 2 },
                new OperationsHistory{Id = 6, OperationDate = DateTime.Now.AddDays(-50), OperationType = OperationType.InputCash, CashSum = 5000m, AccountId = 3 },
                new OperationsHistory{Id = 7, OperationDate = DateTime.Now.AddDays(-30), OperationType = OperationType.InputCash, CashSum = 100m, AccountId = 4 },
                new OperationsHistory{Id = 8, OperationDate = DateTime.Now.AddDays(-30), OperationType = OperationType.InputCash, CashSum = 100m, AccountId = 5 },
                new OperationsHistory{Id = 9, OperationDate = DateTime.Now.AddDays(-70), OperationType = OperationType.InputCash, CashSum = 1000m, AccountId = 9 },
                new OperationsHistory{Id = 10, OperationDate = DateTime.Now.AddDays(-60), OperationType = OperationType.InputCash, CashSum = 500m, AccountId = 9 },
                new OperationsHistory{Id = 11, OperationDate = DateTime.Now.AddDays(-50), OperationType = OperationType.OutputCash, CashSum = 300m, AccountId = 9 },
                new OperationsHistory{Id = 12, OperationDate = DateTime.Now.AddDays(-40), OperationType = OperationType.InputCash, CashSum = 10500m, AccountId = 9 },
                new OperationsHistory{Id = 13, OperationDate = DateTime.Now.AddDays(-30), OperationType = OperationType.OutputCash, CashSum = 1000m, AccountId = 9 },
                new OperationsHistory{Id = 14, OperationDate = DateTime.Now.AddDays(-20), OperationType = OperationType.InputCash, CashSum = 300m, AccountId = 9 }
                }.AsQueryable();

            public void Dispose()
            {

            }
        }
    }
}
