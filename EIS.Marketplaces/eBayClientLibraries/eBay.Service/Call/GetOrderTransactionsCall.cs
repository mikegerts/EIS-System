#region Copyright
//	Copyright (c) 2013 eBay, Inc.
//	
//	This program is licensed under the terms of the eBay Common Development and
//	Distribution License (CDDL) Version 1.0 (the "License") and any subsequent  
//	version thereof released by eBay.  The then-current version of the License can be 
//	found at http://www.opensource.org/licenses/cddl1.php and in the eBaySDKLicense 
//	file that is under the eBay SDK ../docs directory
#endregion

#region Namespaces
using System;
using System.Runtime.InteropServices;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.EPS;
using eBay.Service.Util;

#endregion

namespace eBay.Service.Call
{

	/// <summary>
	/// 
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class GetOrderTransactionsCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public GetOrderTransactionsCall()
		{
			ApiRequest = new GetOrderTransactionsRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public GetOrderTransactionsCall(ApiContext ApiContext)
		{
			ApiRequest = new GetOrderTransactionsRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Retrieves information about one or more orders based on <b>OrderIDs</b>, <b>ItemIDs</b>, or <b>SKU</b> values. <b>Also for Half.com</b>.
		/// </summary>
		/// 
		/// <param name="ItemTransactionIDArrayList">
		/// An array of <b>ItemTransactionIDs</b>.
		/// </param>
		///
		/// <param name="OrderIDArrayList">
		/// An array of <b>OrderIDs</b>. You can specify, at most, twenty <b>OrderIDs</b>.
		/// </param>
		///
		/// <param name="Platform">
		/// The default behavior of <b>GetOrderTransactions</b> is to retrieve all orders originating from eBay.com and Half.com. If the user wants to retrieve only eBay.com order line items or Half.com order line items, this filter can be used to perform that function. Inserting 'eBay' into this field will restrict retrieved order line items to those originating on eBay.com, and inserting 'Half' into this field will restrict retrieved order line items to those originating on Half.com.
		/// </param>
		///
		/// <param name="IncludeFinalValueFees">
		/// Indicates whether to include Final Value Fee (FVF) in the response. For most
		/// listing types, the Final Value Fee is returned in <b>Transaction.FinalValueFee</b>.
		/// The Final Value Fee is returned on a transaction-by-transaction basis for
		/// <b>FixedPriceItem</b> listing type. For all other listing
		/// types, the Final Value Fee is returned when the listing status is Completed.
		/// This value is not returned if the auction ended with Buy It Now.
		/// </param>
		///
		public OrderTypeCollection GetOrderTransactions(ItemTransactionIDTypeCollection ItemTransactionIDArrayList, StringCollection OrderIDArrayList, TransactionPlatformCodeType Platform, bool IncludeFinalValueFees)
		{
			this.ItemTransactionIDArrayList = ItemTransactionIDArrayList;
			this.OrderIDArrayList = OrderIDArrayList;
			this.Platform = Platform;
			this.IncludeFinalValueFees = IncludeFinalValueFees;

			Execute();
			return ApiResponse.OrderArray;
		}



		#endregion




		#region Properties
		/// <summary>
		/// The base interface object.
		/// </summary>
		/// <remarks>This property is reserved for users who have difficulty querying multiple interfaces.</remarks>
		public ApiCall ApiCallBase
		{
			get { return this; }
		}

		/// <summary>
		/// Gets or sets the <see cref="GetOrderTransactionsRequestType"/> for this API call.
		/// </summary>
		public GetOrderTransactionsRequestType ApiRequest
		{ 
			get { return (GetOrderTransactionsRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="GetOrderTransactionsResponseType"/> for this API call.
		/// </summary>
		public GetOrderTransactionsResponseType ApiResponse
		{ 
			get { return (GetOrderTransactionsResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="GetOrderTransactionsRequestType.ItemTransactionIDArray"/> of type <see cref="ItemTransactionIDTypeCollection"/>.
		/// </summary>
		public ItemTransactionIDTypeCollection ItemTransactionIDArrayList
		{ 
			get { return ApiRequest.ItemTransactionIDArray; }
			set { ApiRequest.ItemTransactionIDArray = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="GetOrderTransactionsRequestType.OrderIDArray"/> of type <see cref="StringCollection"/>.
		/// </summary>
		public StringCollection OrderIDArrayList
		{ 
			get { return ApiRequest.OrderIDArray; }
			set { ApiRequest.OrderIDArray = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="GetOrderTransactionsRequestType.Platform"/> of type <see cref="TransactionPlatformCodeType"/>.
		/// </summary>
		public TransactionPlatformCodeType Platform
		{ 
			get { return ApiRequest.Platform; }
			set { ApiRequest.Platform = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="GetOrderTransactionsRequestType.IncludeFinalValueFees"/> of type <see cref="bool"/>.
		/// </summary>
		public bool IncludeFinalValueFees
		{ 
			get { return ApiRequest.IncludeFinalValueFees; }
			set { ApiRequest.IncludeFinalValueFees = value; }
		}
				/// <summary>
		/// Retrieves information about one or more orders or one or more transactions
		/// (or both), thus displacing the need to call GetOrders and GetItemTransactions
		/// separately.
		/// </summary>
		/// 
		/// <param name="ItemTransactionIDArrayList">
		/// An array of ItemTransactionIDs.
		/// </param>
		///
		public OrderTypeCollection GetOrderTransactions(ItemTransactionIDTypeCollection ItemTransactionIDArrayList)
		{
			this.ItemTransactionIDArrayList = ItemTransactionIDArrayList;
			this.OrderIDArrayList = null;

			Execute();
			return ApiResponse.OrderArray;
		}
		/// <summary>
		/// Retrieves information about one or more orders or one or more transactions
		/// (or both), thus displacing the need to call GetOrders and GetItemTransactions
		/// separately.
		/// </summary>
		/// 
		/// <param name="OrderIDArrayList">
		/// An array of OrderIDs.
		/// </param>
		///
		public OrderTypeCollection GetOrderTransactions(StringCollection OrderIDArrayList)
		{
			this.ItemTransactionIDArrayList = null;
			this.OrderIDArrayList = OrderIDArrayList;

			Execute();
			return ApiResponse.OrderArray;
		}
		
 		/// <summary>
		/// Gets the returned <see cref="GetOrderTransactionsResponseType.OrderArray"/> of type <see cref="OrderTypeCollection"/>.
		/// </summary>
		public OrderTypeCollection OrderList
		{ 
			get { return ApiResponse.OrderArray; }
		}
		

		#endregion

		
	}
}
