using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;
using PathWays.API;
using PathWays.Const;
using PathWays.Model;
using PathWays.Model.APIRespone;
namespace PathWays;

public partial class Register : Popup
{

    public Stream filepath ;
    public Register(ImageSource Path , Stream image)
    {
        InitializeComponent();
        filepath = image;
        UploadedOrSelectedImage.Source = Path;
        BindingContext = new Person();
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var viewModel = (Person)BindingContext;
        viewModel.TypeId = 3;
       await CallApiAndPassValues(viewModel);

    }

    private async Task CallApiAndPassValues(Person person)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(person);
                var postResponse = await ApiAsync.CallApi("http://54.89.66.126:80/api/Person", HttpMethod.Post, jsonBody);
                if (postResponse.Item2)
                {
                    GetClockInPerson<PersonInsertResponse> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonInsertResponse>>(postResponse.Item1);
                   var upload= await uploadImageToS3(apiResponse.Data.InsertId);
                    PersonLoginRecord personLoginRecord = new PersonLoginRecord();
                    personLoginRecord.InTime = DateTime.Now;
                    personLoginRecord.Person_Id = apiResponse.Data.InsertId;
                    personLoginRecord.VisitorPersonId = apiResponse.Data.InsertId;
                    personLoginRecord.OutTime = null;
                    string LogInjsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(personLoginRecord);
                    var postLogResponse = await ApiAsync.CallApi("http://54.89.66.126:80/api/PersonLoginRecord", HttpMethod.Post, LogInjsonBody);
                    if (!postLogResponse.Item2)
                    {
                       // await DisplayAlert("Error", "Having somithing issue..", "OK");
                    }
                    UploadedOrSelectedImage.Source = "";
                    BindingContext = new Person();
                    Close(true);
                }
                else
                {
                    Close(false);
                    //await DisplayAlert("Error", "Having somithing issue..", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Close(false);
        }
    }

    public async Task<bool> uploadImageToS3(string Person_Id)
    {
        IAmazonS3 client = new AmazonS3Client(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);

        string destPath = "index/" + Person_Id + ".JPG"; 
        PutObjectRequest request = new PutObjectRequest()
        {
            InputStream = filepath,
            BucketName = ConstantVariables.BacketName,
            Key = destPath,
        };
        request.Metadata.Add(ConstantVariables.TableFieldName2, Person_Id);
        PutObjectResponse response = await client.PutObjectAsync(request);

        return true;

    }
    private void NoButton_Clicked(object sender, EventArgs e)
    {
        Close(false);
    }
}