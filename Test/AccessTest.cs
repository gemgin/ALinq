using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class AccessTest : SqlTest
    {
        private static TextWriter writer;

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            writer = Console.Out;
            return new AccessNorthwind("C:/Northwind.mdb") { Log = writer };
        }

        protected DbConnection CreateConnection()
        {
            return AccessNorthwind.CreateConnection("C:/Northwind.mdb");
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            var type = typeof(SQLiteTest);
            var path = type.Module.FullyQualifiedName;
            var filePath = Path.GetDirectoryName(path) + @"\ALinq.Access.lic";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.Access.lic", filePath);

            writer = new StreamWriter("c:/Access.txt", false);
            var database = new AccessNorthwind("C:/Northwind.mdb") { Log = writer };
            if (!database.DatabaseExists())
            {
                database.CreateDatabase();
                database.Connection.Close();
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            writer.Flush();
            writer.Close();
        }

        public static Func<AccessNorthwind, string, IQueryable<Customer>> CustomersByCity =
            ALinq.CompiledQuery.Compile((AccessNorthwind db, string city) =>
                                        from c in db.Customers
                                        where c.City == city
                                        select c);
        public static Func<AccessNorthwind, string, Customer> CustomersById =
            ALinq.CompiledQuery.Compile((AccessNorthwind db, string id) =>
                                               Enumerable.Where(db.Customers, c => c.CustomerID == id).First());

        [TestMethod]
        public void StoreAndReuseQuery()
        {
            var customers = CustomersByCity((AccessNorthwind)db, "London").ToList();
            Assert.IsTrue(customers.Count() > 0);

            var id = customers.First().CustomerID;
            var customer = CustomersById((AccessNorthwind)db, id);
            Assert.AreEqual("London", customer.City);
        }

       
    }
}
