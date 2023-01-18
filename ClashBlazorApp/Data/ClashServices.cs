using ClashModel;
using Speckle.Newtonsoft.Json;

namespace ClashBlazorApp.Data;

public  class ClashServices
{
    public async Task<ClashTest[]?> GetClashDataAsyn()
    {
        string lasturl = new SpeckleServices().GetLatestCommit().Result;
        var  client =  new  HttpClient();
        var  response = await client.GetAsync( lasturl );
        var  jsonstring = await response.Content.ReadAsStringAsync();
        var  speckleData = JsonConvert.DeserializeObject<List<SpeckleData>>(jsonstring)!;
        return speckleData.FirstOrDefault()!.ClashTests.ToArray();
    }

    public static void SetData()
    {
        throw new NotImplementedException();
    }
}