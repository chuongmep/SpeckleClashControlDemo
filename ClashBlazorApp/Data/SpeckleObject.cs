using ClashModel;
using Speckle.Core.Models;

namespace ClashBlazorApp.Data;

public class SpeckleData  :Base
{
    public List<ClashTest> ClashTests { get; set; }
}