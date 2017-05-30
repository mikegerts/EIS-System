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
	public class RespondToBestOfferCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public RespondToBestOfferCall()
		{
			ApiRequest = new RespondToBestOfferRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public RespondToBestOfferCall(ApiContext ApiContext)
		{
			ApiRequest = new RespondToBestOfferRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// This call enables the seller to accept or decline a buyer's Best Offer on an item, or make a counter offer to the buyer's Best Offer. A seller can decline multiple Best Offers with one call, but the seller cannot accept or counter offer multiple Best Offers with one call. Best Offers are not applicable to auction listings.
		/// </summary>
		/// 
		/// <param name="ItemID">
		/// The unique identifier of the listing to which the seller is responding. Specifies the item for which the BestOffer data is to be returned.
		/// </param>
		///
		/// <param name="BestOfferIDList">
		/// The unique identifier of a buyer's Best Offer for the item. This ID is created once the buyer makes a Best Offer. It is possible that a seller will get multiple Best Offers for an item, and if that seller would like to decline multiple/all of the Best Offers with one <b>RespondToBestOffer</b> call, the seller would pass in each of these identifiers in a separate <b>BestOfferID</b> field. However, the seller can only accept or counter offer one Best Offer at a time.
		/// </param>
		///
		/// <param name="Action">
		/// The enumeration value that the seller passes in to this field will control whether the seller accepts or make a counter offer to a single buyer's Best Offer, or declines one or more buyers' Best Offers. A seller can decline multiple Best Offers with one call, but the seller cannot accept or counter offer multiple Best Offers with one call.
		/// </param>
		///
		/// <param name="SellerResponse">
		/// This optional text field allows the seller to provide more details to the buyer about the action being taken against the buyer's Best Offer.
		/// </param>
		///
		/// <param name="CounterOfferPrice">
		/// The seller inserts counter offer price into this field. This field is conditionally required and only applicable when the <b>Action</b> value is set to <code>Counter</code>, The counter offer price cannot exceed the Buy It Now price for a single quantity item. However, the dollar value in this field may exceed the Buy It Now price if the buyer is requesting or the seller is offering multiple quantity of the item (in a multiple-quantity listing). The quantity of the item must be specified in the <b>CounterOfferQuantity</b> field if the seller is making a counter offer.
		/// </param>
		///
		/// <param name="CounterOfferQuantity">
		/// The seller inserts the quantity of items in the counter offer into this field. This field is conditionally required and only applicable when the <b>Action</b> value is set to <code>Counter</code>, The counter offer price must be specified in the <b>CounterOfferPrice</b> field if the seller is making a counter offer. This price should reflect the quantity of items in the counter offer. So, if the seller's counter offer 'unit' price is 15 dollars, and the item quantity is '2', the dollar value passed into the <b>CounterOfferPrice</b> field would be <code>30.0</code>.
		/// </param>
		///
		public BestOfferTypeCollection RespondToBestOffer(string ItemID, StringCollection BestOfferIDList, BestOfferActionCodeType Action, string SellerResponse, AmountType CounterOfferPrice, int CounterOfferQuantity)
		{
			this.ItemID = ItemID;
			this.BestOfferIDList = BestOfferIDList;
			this.Action = Action;
			this.SellerResponse = SellerResponse;
			this.CounterOfferPrice = CounterOfferPrice;
			this.CounterOfferQuantity = CounterOfferQuantity;

			Execute();
			return ApiResponse.RespondToBestOffer;
		}


		/// <summary>
		/// For backward compatibility with old wrappers.
		/// </summary>
		public BestOfferTypeCollection RespondToBestOffer(string ItemID, StringCollection BestOfferIDList, BestOfferActionCodeType Action, string SellerResponse)
		{
			this.ItemID = ItemID;
			this.BestOfferIDList = BestOfferIDList;
			this.Action = Action;
			this.SellerResponse = SellerResponse;
			Execute();
			return this.RespondToBestOfferList;
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
		/// Gets or sets the <see cref="RespondToBestOfferRequestType"/> for this API call.
		/// </summary>
		public RespondToBestOfferRequestType ApiRequest
		{ 
			get { return (RespondToBestOfferRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="RespondToBestOfferResponseType"/> for this API call.
		/// </summary>
		public RespondToBestOfferResponseType ApiResponse
		{ 
			get { return (RespondToBestOfferResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.ItemID"/> of type <see cref="string"/>.
		/// </summary>
		public string ItemID
		{ 
			get { return ApiRequest.ItemID; }
			set { ApiRequest.ItemID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.BestOfferID"/> of type <see cref="StringCollection"/>.
		/// </summary>
		public StringCollection BestOfferIDList
		{ 
			get { return ApiRequest.BestOfferID; }
			set { ApiRequest.BestOfferID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.Action"/> of type <see cref="BestOfferActionCodeType"/>.
		/// </summary>
		public BestOfferActionCodeType Action
		{ 
			get { return ApiRequest.Action; }
			set { ApiRequest.Action = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.SellerResponse"/> of type <see cref="string"/>.
		/// </summary>
		public string SellerResponse
		{ 
			get { return ApiRequest.SellerResponse; }
			set { ApiRequest.SellerResponse = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.CounterOfferPrice"/> of type <see cref="AmountType"/>.
		/// </summary>
		public AmountType CounterOfferPrice
		{ 
			get { return ApiRequest.CounterOfferPrice; }
			set { ApiRequest.CounterOfferPrice = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="RespondToBestOfferRequestType.CounterOfferQuantity"/> of type <see cref="int"/>.
		/// </summary>
		public int CounterOfferQuantity
		{ 
			get { return ApiRequest.CounterOfferQuantity; }
			set { ApiRequest.CounterOfferQuantity = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="RespondToBestOfferResponseType.RespondToBestOffer"/> of type <see cref="BestOfferTypeCollection"/>.
		/// </summary>
		public BestOfferTypeCollection RespondToBestOfferList
		{ 
			get { return ApiResponse.RespondToBestOffer; }
		}
		

		#endregion

		
	}
}
