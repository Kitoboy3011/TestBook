using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TestBook.Utils;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TestBook.Getmethod
{
    [TestFixture]
    public class GetMethodsTest
    {
        HttpClientUtils httpClientUtils = new HttpClientUtils();
        HttpClient httpClient;
        [SetUp] 
        public void SetUp() 
        {
            //Инициализируем клиент
            httpClient = httpClientUtils.InitHtttpClient();
        }
        [TestCase("Dorne", "true")]
        [TestCase("Dorne", "false")]
        [TestCase("Dorne", "564")]
        [TestCase("The Westerlands", "true")]
        public async Task PositiveGetBook(string region, string hasWords)
        {
            //Задаём Json схему
            string Myschema = @"{
            'url': {'type': 'string'},
            'name': {'type': 'string'},
            'region': {'type': 'string'},
            'coatOfArms': {'type': 'string'},
            'words': {'type': 'string'},
            'titles': {'type': 'array'},
            'seats': {'type': 'array'},
            'currentLord': {'type': 'string'},
            'heir': {'type': 'string'},
            'overlord': {'type': 'string'},
            'founded': {'type': 'string'},
            'founder': {'type': 'string'},
            'diedOut': {'type': 'string'},
            'ancestralWeapons': {'type': 'array'},
            'cadetBranches': {'type': 'array'},
            'swornMembers': {'type': 'array'}
            }";
            //Парсим схему
            var schema = JSchema.Parse(Myschema);
            //Отправляем запрос
            HttpResponseMessage getResponse = await httpClient.GetAsync("?region="+region+"&haswords="+hasWords+"");
            //Записываем ответ на запроса
            var jsonResponse = await getResponse.Content.ReadAsStringAsync();
            //Так как в ответ получаем массив объектов, парсим в Jarray
            JArray arrayObject = JArray.Parse(jsonResponse);
            //Сравниваем объекты массива со схемой
            var isValid = arrayObject.IsValid(schema);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(CheckHasWords(hasWords, arrayObject));
                Assert.IsTrue(CheckRegion(region, arrayObject));
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(isValid, Is.True);

            });
        }
        //Проверка что все элементы массивы содержат поле регион с нужным значением
        private static bool CheckRegion(string region, JArray arrayObject) 
        {
            //Возвращаем true если все элементы содержат region с заданным значением и false если не все
            bool checkRegion = arrayObject.All(item => item.SelectToken("region").ToString() == region);
            return checkRegion;
        }
        //Проверка строки words
        private static bool CheckHasWords(string hasWords, JArray arrayObjecct)
        {
            //Проверяем что строка не пустая если значение True
            if (hasWords == "true" || hasWords == "True")
            {
                //Возвращаем true если все элементы содержат words с не пустой строкой и false если не все
                bool checkHasWords = arrayObjecct.All(item => item.SelectToken("words").ToString() != "");
                return checkHasWords;
            }
            //Проверяем что строка пустая при значении False
            else if (hasWords == "false" || hasWords == "False")
            {
                //Возвращаем true если все элементы содержат words с пустой строкой и false если не все
                bool checkHasWords = arrayObjecct.All(item => item.SelectToken("words").ToString() == "");
                return checkHasWords;
            }
            else
            {
                //Если в параметр hasWords передать любое значение отличное от bool то, выдаёт и пустые
                //и не пустые words соотвественно в случае если у нас не True и не False просто проверяем
                //что у words есть какое то значение
                bool checkHasWords = arrayObjecct.All(item => item.SelectToken("words").ToString() != null);
                return checkHasWords;
            }
        }
        //Маленький негативный кейс
        [TestCase(5100, ExpectedResult = HttpStatusCode.NotFound)]
        public async Task<HttpStatusCode> NegativeGetBook(int n)
        {
            //Делаем запрос к несуществуещей книге
            HttpResponseMessage response = await httpClient.GetAsync("/5100");
            return response.StatusCode;
        }
        [TearDown] 
        public void TearDown() 
        {
            //Закрываем клиента
            httpClientUtils.CloseHttpClient(httpClient);
        }
    }
}
