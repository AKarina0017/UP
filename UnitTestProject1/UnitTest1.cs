using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SQLForm;
namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestLoginUserIsExist()
        {
            string login = "login2";
            string password = "pass2";
            bool expected = true;
            BD sql_BD = new BD();
            bool actual = sql_BD.UserIsExist(login,password);
            Assert.AreEqual(expected, actual, "Пользователь существует");
        }
        [TestMethod]
        public void TestGetFioAndRole()
        {
            int ID = 2;
            string expectedFio = "Воробьев Фёдор Алексеевич";
            string expectedRole = "Техник";
            string actualFio;
            string actualRole;
            BD sql_BD = new BD();
            sql_BD.GetFioRole(ID, out actualFio, out actualRole);
            Assert.AreEqual(expectedFio, actualFio, "ФИО совпадают");
            Assert.AreEqual(expectedRole, actualRole, "Роль совпадают");
        }
        [TestMethod]
        public void TestNumOfOrderders()
        {
            string foundWord = "Ноутбук";
            int expected = 2;
            BD sql_BD = new BD();
            int actual = sql_BD.RequestFindResultCount(foundWord);
            Assert.AreEqual(expected, actual, "Количество найденных записей совпадает");

        }
        [TestMethod]
        public void TestGetUserTypeID()
        {
            int expected = 2;
            BD sql_BD = new BD();
            int actual = sql_BD.GetUserTypeID(2);
            Assert.AreEqual(expected, actual, "Роли пользователей совпадают");
        }
        [TestMethod]
        public void TestRequestIsExist()
        {
            bool expected = true;
            BD sql_BD = new BD();
            bool actual = sql_BD.RequestIsExist(2);
            Assert.AreEqual(expected, actual, "Запись существует");
        }
    }
}
