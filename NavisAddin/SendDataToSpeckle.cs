using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;
using ClashModel;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Application = Autodesk.Navisworks.Api.Application;
using ClashTest = ClashModel.ClashTest;

namespace NavisAddin
{
    [PluginAttribute("MyPlugin", "chuongmep", DisplayName = "Send Clash Speckle")]
    public class SendDataToSpeckle : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve +=
                    new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                SQLitePCL.Batteries.Init();
                List<ClashTest> clashes = new List<ClashTest>();
                Progress progressBar = Application.BeginProgress("Send to Speckle.");
                DocumentClash documentClash = Application.ActiveDocument.GetClash();
                if(documentClash.TestsData==null || documentClash.TestsData.Tests.Count==0) return 0;
                foreach (var savedItem in documentClash.TestsData.Tests)
                {
                    if (savedItem == null) continue;
                    var clash = (Autodesk.Navisworks.Api.Clash.ClashTest) savedItem;
                    clashes.Add(new ClashTest()
                    {
                        Name = clash.DisplayName,
                        Status = clash.Status.ToString(),
                        ClashCount = clash.Children.Count,
                        Guid = clash.Guid.ToString(),
                        LastRun = clash.LastRun,
                        Clashes = GetClashData(clash) ?? new List<Clash>(),
                    });
                }

                progressBar.Update(0.5);
                if (progressBar.IsCanceled)
                {
                    return 0;
                }

                string streamId = "14568be237";
                string commitId = Task.Run(() => RunProcess(streamId, clashes)).Result;
                progressBar.Update(1);
                Application.EndProgress();
                progressBar.Dispose();
                DialogResult dialogResult = MessageBox.Show("Done. Do you want open in speckle?", "Sent to Speckle",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                    Process.Start($"https://speckle.xyz/streams/{streamId}/commits/{commitId}");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return 0;
        }

        void ConvertCompositeToSpeckle()
        {
            //TODO
        }

        private List<Clash> GetClashData(Autodesk.Navisworks.Api.Clash.ClashTest clashTest)
        {
            List<Clash> clashes = new List<Clash>();
            List<ClashResult?> clashResults = new List<ClashResult?>();
            SavedItemCollection itemCollection = clashTest.Children;
            if (!itemCollection.Any()) return clashes;
            foreach (SavedItem savedItem in itemCollection)
            {
                if (savedItem == null) continue;
                if (savedItem is ClashResultGroup g)
                {
                    clashResults.AddRange(g.Children.Select(item => (ClashResult) item));
                }

                if (savedItem is ClashResult)
                {
                    clashResults.Add(savedItem as ClashResult);
                }
            }

            clashResults.ForEach(x => clashes.Add(SetClashInfo(x)));
            return clashes;
        }

        private string GetGridIntersect(ClashResult? clashResult)
        {
            if (clashResult == null) return String.Empty;
            GridSystem gridSystem = Autodesk.Navisworks.Api.Application.ActiveDocument.Grids.ActiveSystem;
            if (gridSystem != null)
            {
                GridIntersection oGridIntersection = gridSystem.ClosestIntersection(clashResult.Center);
                if (oGridIntersection != null)
                    return oGridIntersection.FormatIntersectionDisplayString(clashResult.Center, 0.3048) ??
                           String.Empty;
            }

            return String.Empty;
        }

        private string GetLevel(ClashResult? clashResult)
        {
            if (clashResult == null) return String.Empty;
            GridSystem gridSystem = Autodesk.Navisworks.Api.Application.ActiveDocument.Grids.ActiveSystem;
            if (gridSystem != null)
            {
                GridIntersection oGridIntersection = gridSystem.ClosestIntersection(clashResult.Center);
                if (oGridIntersection != null)
                    return oGridIntersection.Level.DisplayName ?? String.Empty;
            }

            return String.Empty;
        }


        Clash SetClashInfo(ClashResult? clashResult)
        {
            Clash clash = new Clash()
            {
                Name = clashResult.DisplayName,
                Status = clashResult.Status.ToString(),
                Distance = clashResult.Distance,
                Guid = clashResult.Guid.ToString(),
                Level = GetLevel(clashResult) ?? String.Empty,
                GridIntersect = GetGridIntersect(clashResult) ?? String.Empty,
            };
            return clash;
        }

        private async Task<string> RunProcess(string streamId, List<ClashTest> clashTests)
        {
            // define the base object
            Account defaultAccount = AccountManager.GetDefaultAccount();
            var client = new Client(defaultAccount);
            string branchName = "main";
            var branch = client.BranchGet(streamId, branchName).Result;
            ServerTransportV2 transport = new ServerTransportV2(defaultAccount, streamId);
            Base data = new Base();
            data["ClashTests"] = clashTests;
            try
            {
                var objectId = await Operations.Send(data, new List<ITransport>() {transport});
                var commitId = await client.CommitCreate(
                    new CommitCreateInput
                    {
                        streamId = streamId,
                        branchName = branch.name,
                        objectId = objectId,
                        message = $"Send {clashTests.Count} clashes test from Navisworks",
                        sourceApplication = "Navisworks",
                        totalChildrenCount = clashTests.Count,
                    });
                return commitId;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return null;
        }

        Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "System.Runtime.CompilerServices.Unsafe")
            {
                return typeof(System.Runtime.CompilerServices.Unsafe).Assembly;
            }
            return null;
        }
    }
}