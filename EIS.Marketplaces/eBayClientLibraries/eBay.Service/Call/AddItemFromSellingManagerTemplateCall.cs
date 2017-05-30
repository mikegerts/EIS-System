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
	public class AddItemFromSellingManagerTemplateCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddItemFromSellingManagerTemplateCall()
		{
			ApiRequest = new AddItemFromSellingManagerTemplateRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddItemFromSellingManagerTemplateCall(ApiContext ApiContext)
		{
			ApiRequest = new AddItemFromSellingManagerTemplateRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// This is the base request type of the <b>AddItemFromSellingManagerTemplate</b> call, which is used to create listings based on settings contained in a Selling Manager template.
		/// </summary>
		/// 
		/// <param name="SaleTemplateID">
		/// The unique identifier of the Selling Manager template that a seller is  using to create a listing. Unique identifiers of Selling Manager templates can be retrieved programatically by using the <b>GetSellingManagerInventory</b> call.
		/// </param>
		///
		/// <param name="ScheduleTime">
		/// This field allows the user to specify a time in the future when the listing being created becomes active. If this field is not used, the listing will become active immediately upon being created successfully. The time in this field should be specified in GMT format.
		/// </param>
		///
		/// <param name="Item">
		/// This container is intended for specifying setting in the listing that differ from values in the Selling Manager template being used.
		/// 
		/// <span class="tablenote"><strong>Note:</strong>
		/// Only the following three values of <b>ItemType</b> may be used for a <b>AddItemFromSellingManagerTemplate</b> call: <b>CategoryMappingAllowed</b>, <b>PaymentMethods</b>, and <b>PayPalEmailAddress</b>.
		/// </span>
		/// </param>
		///
		public string AddItemFromSellingManagerTemplate(long SaleTemplateID, DateTime ScheduleTime, ItemType Item)
		{
			this.SaleTemplateID = SaleTemplateID;
			this.ScheduleTime = ScheduleTime;
			this.Item = Item;

			Execute();
			return ApiResponse.ItemID;
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
		/// Gets or sets the <see cref="AddItemFromSellingManagerTemplateRequestType"/> for this API call.
		/// </summary>
		public AddItemFromSellingManagerTemplateRequestType ApiRequest
		{ 
			get { return (AddItemFromSellingManagerTemplateRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddItemFromSellingManagerTemplateResponseType"/> for this API call.
		/// </summary>
		public AddItemFromSellingManagerTemplateResponseType ApiResponse
		{ 
			get { return (AddItemFromSellingManagerTemplateResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddItemFromSellingManagerTemplateRequestType.SaleTemplateID"/> of type <see cref="long"/>.
		/// </summary>
		public long SaleTemplateID
		{ 
			get { return ApiRequest.SaleTemplateID; }
			set { ApiRequest.SaleTemplateID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddItemFromSellingManagerTemplateRequestType.ScheduleTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime ScheduleTime
		{ 
			get { return ApiRequest.ScheduleTime; }
			set { ApiRequest.ScheduleTime = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddItemFromSellingManagerTemplateRequestType.Item"/> of type <see cref="ItemType"/>.
		/// </summary>
		public ItemType Item
		{ 
			get { return ApiRequest.Item; }
			set { ApiRequest.Item = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.ItemID"/> of type <see cref="string"/>.
		/// </summary>
		public string ItemID
		{ 
			get { return ApiResponse.ItemID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.StartTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime StartTime
		{ 
			get { return ApiResponse.StartTime; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.EndTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime EndTime
		{ 
			get { return ApiResponse.EndTime; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.Fees"/> of type <see cref="FeeTypeCollection"/>.
		/// </summary>
		public FeeTypeCollection FeeList
		{ 
			get { return ApiResponse.Fees; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.CategoryID"/> of type <see cref="string"/>.
		/// </summary>
		public string CategoryID
		{ 
			get { return ApiResponse.CategoryID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddItemFromSellingManagerTemplateResponseType.Category2ID"/> of type <see cref="string"/>.
		/// </summary>
		public string Category2ID
		{ 
			get { return ApiResponse.Category2ID; }
		}
		

		#endregion

		
	}
}
