using System;

using PX.Data;

namespace VendorRecon
{
  public class TestMaint : PXGraph<TestMaint, VendorRecon>
  {

    public PXFilter<VendorRecon> MasterView;
    public PXFilter<VendorRecon> DetailsView;

    [Serializable]
    public class MasterTable : IBqlTable
    {

    }

    [Serializable]
    public class DetailsTable : IBqlTable
    {

    }


  }
}