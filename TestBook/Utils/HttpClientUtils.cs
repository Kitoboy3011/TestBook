using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBook.Utils
{
    public class HttpClientUtils
    {
        public HttpClient InitHtttpClient()
        {
            try
            {
                //Инициализация http клиента
                HttpClient httpClient = new HttpClient()
                {
                    BaseAddress = new Uri("https://www.anapioficeandfire.com/api/houses"),
                };
                return httpClient;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Не удалось инициализировать клиент", ex);
                return null;
            }
        }
        public void CloseHttpClient(HttpClient httpClient)
        {
            //Закрытие http клиента
            httpClient.Dispose();
        }
    }
}
