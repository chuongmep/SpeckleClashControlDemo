using Speckle.Core.Api;
using Speckle.Core.Credentials;

namespace ClashBlazorApp.Data;

public class SpeckleServices
{
    public async Task<string> GetLatestCommit()
    {
        var streamId = "14568be237";
        var branchName = "main";
        Account Account = new Account()
        {
            token = "7538e7c3aa5c2763217408a0d0d46c5c72874d2b85",
        };
        Account.serverInfo = new ServerInfo
        {
            url = "https://speckle.xyz/"
        };
        //Account defaultAccount = AccountManager.GetDefaultAccount();
        Client client = new Client(Account);
        if (client == null) throw new ArgumentException(nameof(client));
        var branch = client.BranchGet(streamId, branchName, 1).Result;
        if(branch == null) throw new ArgumentException(nameof(branch));
        // take last object commit
        if(branch.commits.totalCount<=0) return await Task.FromResult(String.Empty);
        var objectId = branch.commits.items[0].referencedObject; 
        if(objectId == null) throw new ArgumentException(nameof(objectId));
        string url = $"https://speckle.xyz/objects/{streamId}/{objectId}";
        return await Task.FromResult(url);
    }
}