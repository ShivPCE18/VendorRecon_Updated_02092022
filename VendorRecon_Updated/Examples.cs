using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using VendorRecon_Updated.DAC;

namespace MyProject
{
	#region Overriding an existing graph
	/// <summary>
	///To override or extend the behavior of an existing graph,
	///derive a new class from the PXGraphExtension<T> class where T is the original graph type.
	/// </summary>
	/// 
	public class InventoryItemMaintExtension : PXGraphExtension<InventoryItemMaint>
	{
		protected void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
		{
			//execute the event handler of the base graph
			if (del != null)
				del(sender, e);
			//implement the extension code
		}
	}
	#endregion

	#region Declaring a new graph
	/// <summary>
	///To declare a new graph for a new custom page, 
	///derive the new graph class from PXGraph<T> or PXGraph<T, A> where
	///T is the type of the new graph (required), 
	///A is the type of the data access class of the primary view of the graph (optional).
	/// </summary>
	public class SampleGraph : PXGraph<SampleGraph>
	{
		//Declare the data views and implement the event handlers here     
	}
	#endregion

	#region Extending an exising data access class
	/// <summary>
	///To extend an exising data access class,
	///derive a new class from the PXCacheExtension<T> class where T is the original data access class type.
	/// </summary>
	public class ProcessingFilter : IBqlTable
	{
        #region MyCustomField
        public abstract class Fyear : PX.Data.BQL.BqlString.Field<Fyear> { }
        [PXString(255)]
		[PXStringList(
		   new string[]
		   {
			ReconciliationConstants.Open,
			ReconciliationConstants.OnHold,
			ReconciliationConstants.Completed,
		   },
		   new string[]
		   {
			VendorRecon_Updated.DAC.Messages.Open,
			VendorRecon_Updated.DAC.Messages.OnHold,
			VendorRecon_Updated.DAC.Messages.Completed,
		   }
		   )]
		public virtual string Status
        {
            get;
            set;
        }
        #endregion
    }
    #endregion

    #region Declaring a new data access class
    /// <summary>
    ///To declare a new data access class, 
    ///declare the new class as one that implements the IBqlTable interface and add the [Serializable] to the class.
    /// </summary>
    [Serializable]
	public class MyCustomTable : IBqlTable
	{
	}
	#endregion
}