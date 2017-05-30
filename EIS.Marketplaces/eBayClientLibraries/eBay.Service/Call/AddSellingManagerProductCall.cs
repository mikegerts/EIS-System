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
	public class AddSellingManagerProductCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddSellingManagerProductCall()
		{
			ApiRequest = new AddSellingManagerProductRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddSellingManagerProductCall(ApiContext ApiContext)
		{
			ApiRequest = new AddSellingManagerProductRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// The base request type of the <b>AddSellingManagerProduct</b> call, which is used to create a product or a group of product variations within the Selling Manager Pro system. Once a Selling Manager Pro product or production variation group is created, the product settings and product specifics can be transferred over into a Selling Manager Pro listing template with the <b>AddSellingManagerTemplate</b> call.
		/// </summary>
		/// 
		/// <param name="SellingManagerProductDetails">
		/// This container is used to provide details about the Selling Manager product, such as product name, quantity available, and unit price.
		/// </param>
		///
		/// <param name="FolderID">
		/// This is the unique identifier of the folder in which the new product will be placed. This folder can be a new folder or a folder that already exists for the seller in Selling Manager Pro. If no folder is specified through this field, the new product is place into the <em>My Products</em> folder by default.
		/// </param>
		///
		/// <param name="SellingManagerProductSpecifics">
		/// This container allows the seller to specify item specifics for a product, to create a product variation group and variation specifics, and/or to specify a listing category for the product or product variation group. A product variation group can be transferred into a listing template that can create a multiple-variation listing. The listing category can either be provided through the <b>PrimaryCategoryID</b> value of this call, or through the <b>Item.PrimaryCategory.CategoryID</b> field of the subsequent <b>AddSellingManagerTemplate</b> call.
		/// </param>
		///
		public SellingManagerProductDetailsType AddSellingManagerProduct(SellingManagerProductDetailsType SellingManagerProductDetails, long FolderID, SellingManagerProductSpecificsType SellingManagerProductSpecifics)
		{
			this.SellingManagerProductDetails = SellingManagerProductDetails;
			this.FolderID = FolderID;
			this.SellingManagerProductSpecifics = SellingManagerProductSpecifics;

			Execute();
			return ApiResponse.SellingManagerProductDetails;
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
		/// Gets or sets the <see cref="AddSellingManagerProductRequestType"/> for this API call.
		/// </summary>
		public AddSellingManagerProductRequestType ApiRequest
		{ 
			get { return (AddSellingManagerProductRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddSellingManagerProductResponseType"/> for this API call.
		/// </summary>
		public AddSellingManagerProductResponseType ApiResponse
		{ 
			get { return (AddSellingManagerProductResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerProductRequestType.SellingManagerProductDetails"/> of type <see cref="SellingManagerProductDetailsType"/>.
		/// </summary>
		public SellingManagerProductDetailsType SellingManagerProductDetails
		{ 
			get { return ApiRequest.SellingManagerProductDetails; }
			set { ApiRequest.SellingManagerProductDetails = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerProductRequestType.FolderID"/> of type <see cref="long"/>.
		/// </summary>
		public long FolderID
		{ 
			get { return ApiRequest.FolderID; }
			set { ApiRequest.FolderID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddSellingManagerProductRequestType.SellingManagerProductSpecifics"/> of type <see cref="SellingManagerProductSpecificsType"/>.
		/// </summary>
		public SellingManagerProductSpecificsType SellingManagerProductSpecifics
		{ 
			get { return ApiRequest.SellingManagerProductSpecifics; }
			set { ApiRequest.SellingManagerProductSpecifics = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="AddSellingManagerProductResponseType.SellingManagerProductDetails"/> of type <see cref="SellingManagerProductDetailsType"/>.
		/// </summary>
		public SellingManagerProductDetailsType SellingManagerProductDetailsReturn
		{ 
			get { return ApiResponse.SellingManagerProductDetails; }
		}
		

		#endregion

		
	}
}
