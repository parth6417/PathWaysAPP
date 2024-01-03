using CommunityToolkit.Maui.Views;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using PathWays.Model;
using PathWays.Model.APIRespone;
using Newtonsoft.Json;
using PathWays.API;
using PathWays.Const;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PathWays
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private  void OnCounterClicked(object sender, EventArgs e)
        {
            cameraView.Camera = cameraView.Cameras.FirstOrDefault( i=> i.Position == Camera.MAUI.CameraPosition.Front);
            MainThread.BeginInvokeOnMainThread(async () =>
            {

                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            }) ;
        }


       

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var loder= new Loder();
            this.ShowPopup(loder);
            cameraButton.IsEnabled = false;
            var cameraCaptureResult = await cameraView.TakePhotoAsync(Camera.MAUI.ImageFormat.PNG);

            var image = cameraView.GetSnapShot(Camera.MAUI.ImageFormat.PNG);
            if (cameraCaptureResult != null)
            {
                try
                {
                    var rekognitionClient = new AmazonRekognitionClient(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);
                    var dynamoDBClient = new AmazonDynamoDBClient(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);
                    using (var imageStream = cameraCaptureResult)
                    {
                        var imageBytes = new byte[imageStream.Length];
                        imageStream.Read(imageBytes, 0, imageBytes.Length);
                        var resizedImageBytes = ResizeImage(imageBytes, maxWidth: 800, maxHeight: 600);
                        var searchFacesRequest = new SearchFacesByImageRequest
                        {
                            CollectionId = ConstantVariables.CollectionId,
                            Image = new Amazon.Rekognition.Model.Image
                            {
                                Bytes = new MemoryStream(resizedImageBytes)
                            }
                        };

                        var response = await rekognitionClient.SearchFacesByImageAsync(searchFacesRequest);

                        bool found = false;

                        foreach (var match in response.FaceMatches)
                        {
                            var getFaceRequest = new GetItemRequest
                            {
                                TableName = ConstantVariables.TableName,
                                Key = new Dictionary<string, AttributeValue> { { ConstantVariables.TableFieldName1, new AttributeValue { S = match.Face.FaceId } } }
                            };

                            var faceResponse = await dynamoDBClient.GetItemAsync(getFaceRequest);

                            if (faceResponse.Item.Count() != 0)
                            {
                                Console.WriteLine($"Found Person: {faceResponse.Item["FullName"].S}");
                                found = true;
                                var getResponse = await ApiAsync.CallApi("http://54.89.66.126:80/api/PersonLoginRecord/GetClockInPersonById/" + faceResponse.Item["FullName"].S, HttpMethod.Get);

                                if (getResponse.Item2)
                                {
                                    GetClockInPerson<PersonLoginRecord> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonLoginRecord>>(getResponse.Item1);
                                    if (apiResponse.Data != null)
                                    {
                                        if (apiResponse.Data?.OutTime == null)
                                        {
                                            var SaveResponse = await UpdateData(apiResponse?.Data);
                                            if (SaveResponse)
                                            {
                                                var tost = Toast.Make("Log Out successfully", ToastDuration.Long);
                                                await tost.Show();
                                            }
                                            else
                                                await DisplayAlert("Error", "Having somithing issue..", "OK");
                                        }
                                    }
                                    else
                                    {
                                        var SaveResponse= await SaveData(faceResponse.Item["FullName"].S);
                                        if (SaveResponse)
                                        {
                                            var tost = Toast.Make("Log in successfully", ToastDuration.Long);
                                            await tost.Show();
                                        }
                                        else
                                            await DisplayAlert("Error", "Having somithing issue..", "OK");
                                    }
                                }
                            }
                        }
                        if (!found)
                        {   
                            var registerpopup = new Register(image , cameraCaptureResult);
                            var data = await this.ShowPopupAsync(registerpopup);

                            if (Convert.ToBoolean(data))
                            {
                                var tost = Toast.Make("Register Sucessed.....", ToastDuration.Long);
                                await tost.Show();
                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }

            cameraButton.IsEnabled = true;
            loder.Close();
        }


        private byte[] ResizeImage(byte[] originalImageBytes, int maxWidth, int maxHeight)
        {
            using (var originalStream = new MemoryStream(originalImageBytes))
            using (var originalBitmap = SkiaSharp.SKBitmap.Decode(originalStream))
            {
                var resizedBitmap = originalBitmap.Resize(new SkiaSharp.SKImageInfo(maxWidth, maxHeight), SkiaSharp.SKFilterQuality.High);
                using (var resizedStream = new MemoryStream())
                {
                    resizedBitmap.Encode(resizedStream, SkiaSharp.SKEncodedImageFormat.Png, 100);
                    return resizedStream.ToArray();
                }
            }
        }

        public async Task<bool> SaveData(string id)
        {
            try
            {
                PersonLoginRecord personLoginRecord = new PersonLoginRecord();
                personLoginRecord.InTime = DateTime.Now;
                personLoginRecord.Person_Id = id;
                personLoginRecord.VisitorPersonId = id;
                personLoginRecord.OutTime = null;
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(personLoginRecord);
                var postResponse = await ApiAsync.CallApi("http://54.89.66.126:80/api/PersonLoginRecord", HttpMethod.Post, jsonBody);
                if (postResponse.Item2)
                {
                    return true; 
                }
                else
                {
                    return false; 
                }
            }catch
            (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateData(PersonLoginRecord Data)
        {
            try
            {
                PersonLoginRecord personLoginRecord = new PersonLoginRecord();
                personLoginRecord.Id = Data.Id;
                personLoginRecord.InTime = Data?.InTime;
                personLoginRecord.Person_Id = Data?.Person_Id;
                personLoginRecord.VisitorPersonId = Data?.Person_Id;
                personLoginRecord.OutTime = DateTime.Now;
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(personLoginRecord);
                var postResponse = await ApiAsync.CallApi("http://54.89.66.126:80/api/PersonLoginRecord", HttpMethod.Put, jsonBody);
                if (postResponse.Item2)
                    return true;
                else
                    return false;
            }
            catch
            (Exception ex)
            {
                return false;
            }
        }
    }

    

}
