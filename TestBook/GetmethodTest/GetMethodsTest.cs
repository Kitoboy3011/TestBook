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
            //Сравниваем объекты массива со схемоу
            var isValid = arrayObject.IsValid(schema);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(CheckRegion(region, arrayObject));
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(isValid, Is.True);

            });
        }
        private static bool CheckRegion(string region, JArray arrayObject) 
        {

            foreach (JObject item in arrayObject)
            {
                //Находим у объектов массива значение региона
                var regionValue = item.GetValue("region");
                //Сравниваем значение с тем которое было задано
                if (regionValue.ToString() == region)
                {
                    return true;
                }
                return false;
            }
            return false;
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
