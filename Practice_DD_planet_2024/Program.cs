using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Practice_DD_planet
{
    // класс для таблицы пользователей
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public List<Role> Roles { get; set; }
    } 
    // класс для таблицы ролей
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
    } 
    class Program
    {
        static void Main(string[] args)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString))
            {
                // текст sql-запроса
                var sqlQuery = "SELECT * FROM Users INNER JOIN UserRole ON Users.id = UserRole.UserID INNER JOIN Roles ON Roles.id = UserRole.RoleID;";

                // словари объектов классов пользователей и ролей, в качестве ключа в обоих случаях выступает Id
                var userDictionary = new Dictionary<int, User>();
                var roleDictionary = new Dictionary<int, Role>();

                // запрос
                var allUserRole = db.Query<User, Role, User>(sqlQuery, (user, role) =>
                {
                    User userEntry;
                    if (!userDictionary.TryGetValue(user.Id, out userEntry))
                    {
                        // если пользователя user нет в словаре, то добавить его и инициализировать его поле Roles
                        userEntry = user;
                        userEntry.Roles = new List<Role>();
                        userDictionary.Add(userEntry.Id, userEntry);
                    }
                    

                    Role roleEntry;
                    if (!roleDictionary.TryGetValue(role.Id, out roleEntry))
                    {
                        // если роли role нет в словаре, то добавить её и инициализировать её поле Users
                        roleEntry = role;
                        roleEntry.Users = new List<User>();
                        roleDictionary.Add(roleEntry.Id, roleEntry);
                    }

                    // добавить для пользователя userEntry роль roleEntry, которую он исполняет
                    userEntry.Roles.Add(roleEntry);
                    // добавить для роли roleEntry пользователя userEntry, который входит в эту роль
                    roleEntry.Users.Add(userEntry);
                    return userEntry;
                }, splitOn: "Id").Distinct().ToList();

                Console.WriteLine("1) Вывод всех пользователей и ролей, в которых они состоят");
                Console.WriteLine($"Число пользователей: {allUserRole.Count}");
                foreach (var user in allUserRole)
                {
                    Console.Write($"id: {user.Id}\tИмя: {user.Name}\tФамилия: {user.Surname}\t - ");
                    foreach (var role in user.Roles) Console.Write($"{role.Name} ");
                    Console.WriteLine();
                }
                Console.WriteLine();

                Console.WriteLine("2) Определение количества пользователей по каждой роли");

                foreach (var role in roleDictionary)
                    Console.WriteLine($"id роли: {role.Key}\tРоль: {role.Value.Name}\t - {role.Value.Users.Count} users");
                Console.WriteLine();
                Console.ReadKey();
            }
        }
    }
}
