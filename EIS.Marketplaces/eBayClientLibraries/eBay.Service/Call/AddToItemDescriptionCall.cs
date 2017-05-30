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
	public class AddToItemDescriptionCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddToItemDescriptionCall()
		{
			ApiRequest = new AddToItemDescriptionRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddToItemDescriptionCall(ApiContext ApiContext)
		{
			ApiRequest = new AddToItemDescriptionRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// The base request of the <b>AddToItemDescription</b> call, which is used to add additional text to an active listing's item description. Upon a successful call, the text supplied in this call will be inserted into the active listing, with a horizontal rule separating the original item description and the additional text that was added with this call.
		/// </summary>
		/// 
		/// <param name="ItemID">
		/// This field is used to identify the active listing that will be updated with additional text in the item description field. The Item ID is a unique identifier for a listing.
		/// </param>
		///
		/// <param name="DescriptionToAppend">
		/// This field is used to specify the text that will be appended to the end of the active listing's item description.
		/// The text provided in this field must abide by the same rules/restrictions
		/// applicable to the original item description supplied at listing time. For more information on these rules and restrictions, see the <a href="http://developer.ebay.com/Devzone/XML/docs/Reference/ebay/types/ItemType.html#Description">Description field of ItemType</a>.
		/// </param>
		///
		public void AddToItemDescription(string ItemID, string DescriptionToAppend)
		{
			this.ItemID = ItemID;
			this.DescriptionToAppend = DescriptionToAppend;

			Execute();
			
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
		/// Gets or sets the <see cref="AddToItemDescriptionRequestType"/> for this API call.
		/// </summary>
		public AddToItemDescriptionRequestType ApiRequest
		{ 
			get { return (AddToItemDescriptionRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddToItemDescriptionResponseType"/> for this API call.
		/// </summary>
		public AddToItemDescriptionResponseType ApiResponse
		{ 
			get { return (AddToItemDescriptionResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddToItemDescriptionRequestType.ItemID"/> of type <see cref="string"/>.
		/// </summary>
		public string ItemID
		{ 
			get { return ApiRequest.ItemID; }
			set { ApiRequest.ItemID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddToItemDescriptionRequestType.Description"/> of type <see cref="string"/>.
		/// </summary>
		public string DescriptionToAppend
		{ 
			get { return ApiRequest.Description; }
			set { ApiRequest.Description = value; }
		}
		
		

		#endregion

		
	}
}
