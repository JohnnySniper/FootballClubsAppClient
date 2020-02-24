using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

namespace FootballClubs
{
    public static class ServerConnection
    {
        public const string server = "https://localhost:5001/";
        private static HttpClient client;

        public static void CreateClient()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        }

        public static bool SaveNewClub(FootballClub club)
        {
            if (string.IsNullOrWhiteSpace(club.Name))
            {
                MainWindow.ShowMessageBox("Некорректные данные" ,"Поле с названием не должно содержать пустых значений или состоять из символов-разделителей!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(club.ImagePath))
            {
                MainWindow.ShowMessageBox("Некорректные данные", "Необходимо выбрать путь к файлу!");
                return false;
            }

            FileStream fs = new FileStream(club.ImagePath, FileMode.Open, FileAccess.Read);
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fs), "image", "image");

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, server + "Clubs/Add/" + club.Id + "/" + club.Name);
            httpRequest.Content = content;

            var response = client.SendAsync(httpRequest).Result;

            fs.Close();

            if (response.IsSuccessStatusCode)
                return true;

            MainWindow.ShowMessageBox("Редактирование данных", response.StatusCode.ToString());
            return false;
        }

        public static bool SaveEditedClub(FootballClub club)
        {
            if (string.IsNullOrWhiteSpace(club.Name))
            {
                MainWindow.ShowMessageBox("Некорректные данные", "Поле с названием не должно содержать пустых значений или состоять из символов-разделителей!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(club.ImagePath))
            {
                MainWindow.ShowMessageBox("Некорректные данные", "Необходимо выбрать путь к файлу!");
                return false;
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server + "Clubs/Edit/" + club.Id);

            FileStream fs = new FileStream(club.ImagePath, FileMode.Open, FileAccess.Read);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(club.Name), "name", "name");
            content.Add(new StreamContent(fs), "image", "image");
            request.Content = content;

            var response = client.SendAsync(request).Result;

            fs.Close();

            if(response.IsSuccessStatusCode)
                return true;

            MainWindow.ShowMessageBox("Редактирование данных", response.StatusCode.ToString());
            return false;
        }

        public static string RemoveClub(int id)
        {
            var response = client.DeleteAsync(server + "Clubs/Remove/" + id).Result;
            return response.ReasonPhrase;
        }

        public static List<FootballClub> LoadClubs()
        {
            HttpResponseMessage response = null;
            List<FootballClub> clubs = new List<FootballClub>();
            try
            {
                response = client.GetAsync(server + "Clubs/Load").Result;
            }catch(Exception e)
            {
                MainWindow.ShowMessageBox("Подключение", "Соединение с сервером отсутствует\n" + e.Message);
                App.Current.Shutdown();
                return clubs;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                clubs = new JavaScriptSerializer().Deserialize<List<FootballClub>>(response.Content.ReadAsStringAsync().Result);
            }
            
            return clubs;
        }

        public static string LoadImage(FootballClub club)
        {
            string path = Directory.GetCurrentDirectory() + "\\Images";
            string imageName = club.Name + "_logo.png";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += "\\" + imageName;

            var response = client.GetAsync(server + "Clubs/GetLogo/" + club.Id).Result.Content.ReadAsStreamAsync().Result;

            if (response != null)
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                response.CopyToAsync(fs).Wait();
                response.Close();
                fs.Close();
            }
            return path;
        }
    }
}
