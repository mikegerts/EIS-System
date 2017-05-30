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
	public class AddOrderCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddOrderCall()
		{
			ApiRequest = new AddOrderRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddOrderCall(ApiContext ApiContext)
		{
			ApiRequest = new AddOrderRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// The <b>AddOrder</b> call can be used by a seller to combine two or more unpaid, single line item orders from the same buyer into one 'Combined Invoice' order with multiple line items. Once multiple line items are combined into one order, the buyer can make one single payment for each line item in the order. If possible and agreed to, the seller can then ship multiple line items in the same shipping package, saving on shipping costs, and possibly passing that savings down to the buyer through Combined Shipping Discount rules set up in My eBay.
		/// </summary>
		/// 
		/// <param name="Order">
		/// The root container of the <b>AddOrder</b> request. In this
		/// call, the seller identifies two or more unpaid order line items from the same buyer
		/// through the <b>TransactionArray</b> container, specifies one or
		/// more accepted payment methods through the <b>PaymentMethods</b>
		/// field(s), and specifies available shipping services and other shipping details
		/// through the <b>ShippingDetails</b> container.
		/// </param>
		///
		public string AddOrder(OrderType Order)
		{
			this.Order = Order;

			Execute();
			return ApiResponse.OrderID;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Execute()
		{
			base.Execute();
			Order.OrderID = ApiResponse.OrderID;
			Order.CreatedTime = ApiResponse.CreatedTime;
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
		/// Gets or sets the <see cref="AddOrderRequestType"/> for this API call.
		/// </summary>
		public AddOrderRequestType ApiRequest
		{ 
			get { return (AddOrderRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddOrderResponseType"/> for this API call.
		/// </summary>
		public AddOrderResponseType ApiResponse
		{ 
			get { return (AddOrderResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddOrderRequestType.Order"/> of type <see cref="OrderType"/>.
		/// </summary>
		public OrderType Order
		{ 
			get { return ApiRequest.Order; }
			set { ApiRequest.Order = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="AddOrderResponseType.OrderID"/> of type <see cref="string"/>.
		/// </summary>
		public string OrderID
		{ 
			get { return ApiResponse.OrderID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddOrderResponseType.CreatedTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime CreatedTime
		{ 
			get { return ApiResponse.CreatedTime; }
		}
		

		#endregion

		
	}
}
