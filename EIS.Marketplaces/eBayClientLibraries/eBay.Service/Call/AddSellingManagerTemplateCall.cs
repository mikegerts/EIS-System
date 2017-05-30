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
	public class AddSellingManagerTemplateCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddSellingManagerTemplateCall()
		{
			ApiRequest = new AddSellingManagerTemplateRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddSellingManagerTemplateCall(ApiContext ApiContext)
		{
			ApiRequest = new AddSellingManagerTemplateRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// The base request for the <b>AddSellingManagerTemplate</b> call, which is used to create a Selling Manager listing template. Each Selling Manager listing template must be associated with an existing Selling Manager product, and each product can have up to 20 listing templates associated with it.
		/// </summary>
		/// 
		/// <param name="Item">
		/// This <b>Item</b> container is similar to the one that is used in standard Add/Revise/Relist/Verify Item calls, with the difference being that some of the product-specific data/settings will be inherited through the Selling Manager product that is referenced in the call request, and not through this <b>Item</b> container. The data that is passed in this container will become part of the listing template that is created with this call.
		/// </param>
		///
		/// <param name="SaleTemplateName">
		/// The name of the Selling Manager listing template is provided in this field. If you don't specify
		/// a name, then the value in the <b>Item.Title</b> field will be used as the name instead.
		/// </param>
		///
		/// <param name="ProductID">
		/// The unique identifier of the Selling Manager product that will be associated with the listing template being created. Selling Manager Product IDs are returned in the response of a <b>AddSellingManagerProduct</b> call.
		/// Alternatively, the <b>GetSellingManagerInventory</b> call can be used to retrieve active Selling Manager products for the user's account.
		/// </param>
		///
		public long AddSellingManagerTemplate(ItemType Item, string SaleTemplateName, long ProductID)
		{
			this.Item = Item;
			this.SaleTemplateName = SaleTemplateName;
			this.ProductID = ProductID;

			Execute();
			return ApiResponse.CategoryID;
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
		/// Gets or sets the <see cref="AddSellingManagerTemplateRequestType"/> for this API call.
		/// </summary>
		public AddSellingManagerTemplateRequestType ApiRequest
		{ 
			get { return (AddSellingManagerTemplateRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddSellingManagerTemplateResponseType"/> for this API call.
		/// </summary>
		public AddSellingManagerTemplateResponseType ApiResponse
		{ 
			get { return (AddSellingManagerTemplateResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerTemplateRequestType.Item"/> of type <see cref="ItemType"/>.
		/// </summary>
		public ItemType Item
		{ 
			get { return ApiRequest.Item; }
			set { ApiRequest.Item = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerTemplateRequestType.SaleTemplateName"/> of type <see cref="string"/>.
		/// </summary>
		public string SaleTemplateName
		{ 
			get { return ApiRequest.SaleTemplateName; }
			set { ApiRequest.SaleTemplateName = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerTemplateRequestType.ProductID"/> of type <see cref="long"/>.
		/// </summary>
		public long ProductID
		{ 
			get { return ApiRequest.ProductID; }
			set { ApiRequest.ProductID = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.CategoryID"/> of type <see cref="long"/>.
		/// </summary>
		public long CategoryID
		{ 
			get { return ApiResponse.CategoryID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.Category2ID"/> of type <see cref="long"/>.
		/// </summary>
		public long Category2ID
		{ 
			get { return ApiResponse.Category2ID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.SaleTemplateID"/> of type <see cref="long"/>.
		/// </summary>
		public long SaleTemplateID
		{ 
			get { return ApiResponse.SaleTemplateID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.SaleTemplateGroupID"/> of type <see cref="long"/>.
		/// </summary>
		public long SaleTemplateGroupID
		{ 
			get { return ApiResponse.SaleTemplateGroupID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.SaleTemplateName"/> of type <see cref="string"/>.
		/// </summary>
		public string SaleTemplateNameReturn
		{ 
			get { return ApiResponse.SaleTemplateName; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.SellingManagerProductDetails"/> of type <see cref="SellingManagerProductDetailsType"/>.
		/// </summary>
		public SellingManagerProductDetailsType SellingManagerProductDetails
		{ 
			get { return ApiResponse.SellingManagerProductDetails; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerTemplateResponseType.Fees"/> of type <see cref="FeeTypeCollection"/>.
		/// </summary>
		public FeeTypeCollection FeeList
		{ 
			get { return ApiResponse.Fees; }
		}
		

		#endregion

		
	}
}
