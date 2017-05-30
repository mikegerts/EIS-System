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
	public class AddTransactionConfirmationItemCall : ApiCall
	{

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AddTransactionConfirmationItemCall()
		{
			ApiRequest = new AddTransactionConfirmationItemRequestType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ApiContext">The <see cref="ApiCall.ApiContext"/> for this API Call of type <see cref="ApiContext"/>.</param>
		public AddTransactionConfirmationItemCall(ApiContext ApiContext)
		{
			ApiRequest = new AddTransactionConfirmationItemRequestType();
			this.ApiContext = ApiContext;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Base request of the <b>AddTransactionConfirmationItem</b> call, which is used to end an eBay Motors listing and creates a new Transaction Confirmation Request (TCR) for the motor vehichle, thus enabling the TCR recipient to purchase the item. You can also use this call to see if a new TCR can be created for the specified item.
		/// </summary>
		/// 
		/// <param name="RecipientUserID">
		/// This field is used to specify the recipient of the Transaction
		/// Confirmation Request (TCR).
		/// </param>
		///
		/// <param name="VerifyEligibilityOnly">
		/// This field is included and set to <code>true</code> if the seller wants to verify whether or not a new Transaction
		/// Confirmation Request (TCR) can be created for the item. If this field is included and its value set to <code>true</code>, no TCR is actually created. If this field is included and its value set to <code>false</code>, or if it is omitted, a Transaction
		/// Confirmation Request is actually created.
		/// </param>
		///
		/// <param name="RecipientPostalCode">
		/// This field is used to specify the postal code of the user to whom the seller is offering the Transaction Confirmation Request. This field is only required if the user does not meet the other options listed in <b>RecipientRelationCodeType</b>.
		/// </param>
		///
		/// <param name="RecipientRelationType">
		/// The enumeration value supplied in this field specifies the current relationship between the seller and the potential
		/// buyer. A Transaction Confirmation Request (TCR) for an
		/// item can be sent to a potential buyer who has at least one of the following criteria: is an active bidder on the auction listing; has made a Best Offer on the fixed-price listing, is an eBay user who has used the Ask Seller a Question feature, or is an eBay user whose postal code is known.
		/// </param>
		///
		/// <param name="NegotiatedPrice">
		/// The amount in this field is the price that the seller is asking for to purchase the motor vehicle.
		/// </param>
		///
		/// <param name="ListingDuration">
		/// The enumeration value specified in this field will control how many days that the recipient of the offer has to purchase the motor vehicle at the price listed in the <b>NegotiatedPrice</b> field.
		/// </param>
		///
		/// <param name="ItemID">
		/// This field is used to identify the eBay Motors listing using the unique identifier of the listing (<b>ItemID</b>).
		/// </param>
		///
		/// <param name="Comments">
		/// This is optional free-form string field that can be used by the seller to provide any comments or additional information about the Transaction Confirmation Item.
		/// </param>
		///
		public string AddTransactionConfirmationItem(string RecipientUserID, string VerifyEligibilityOnly, string RecipientPostalCode, RecipientRelationCodeType RecipientRelationType, AmountType NegotiatedPrice, SecondChanceOfferDurationCodeType ListingDuration, string ItemID, string Comments)
		{
			this.RecipientUserID = RecipientUserID;
			this.VerifyEligibilityOnly = VerifyEligibilityOnly;
			this.RecipientPostalCode = RecipientPostalCode;
			this.RecipientRelationType = RecipientRelationType;
			this.NegotiatedPrice = NegotiatedPrice;
			this.ListingDuration = ListingDuration;
			this.ItemID = ItemID;
			this.Comments = Comments;

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
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType"/> for this API call.
		/// </summary>
		public AddTransactionConfirmationItemRequestType ApiRequest
		{ 
			get { return (AddTransactionConfirmationItemRequestType) AbstractRequest; }
			set { AbstractRequest = value; }
		}

		/// <summary>
		/// Gets the <see cref="AddTransactionConfirmationItemResponseType"/> for this API call.
		/// </summary>
		public AddTransactionConfirmationItemResponseType ApiResponse
		{ 
			get { return (AddTransactionConfirmationItemResponseType) AbstractResponse; }
		}

		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.RecipientUserID"/> of type <see cref="string"/>.
		/// </summary>
		public string RecipientUserID
		{ 
			get { return ApiRequest.RecipientUserID; }
			set { ApiRequest.RecipientUserID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.VerifyEligibilityOnly"/> of type <see cref="string"/>.
		/// </summary>
		public string VerifyEligibilityOnly
		{ 
			get { return ApiRequest.VerifyEligibilityOnly; }
			set { ApiRequest.VerifyEligibilityOnly = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.RecipientPostalCode"/> of type <see cref="string"/>.
		/// </summary>
		public string RecipientPostalCode
		{ 
			get { return ApiRequest.RecipientPostalCode; }
			set { ApiRequest.RecipientPostalCode = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.RecipientRelationType"/> of type <see cref="RecipientRelationCodeType"/>.
		/// </summary>
		public RecipientRelationCodeType RecipientRelationType
		{ 
			get { return ApiRequest.RecipientRelationType; }
			set { ApiRequest.RecipientRelationType = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.NegotiatedPrice"/> of type <see cref="AmountType"/>.
		/// </summary>
		public AmountType NegotiatedPrice
		{ 
			get { return ApiRequest.NegotiatedPrice; }
			set { ApiRequest.NegotiatedPrice = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.ListingDuration"/> of type <see cref="SecondChanceOfferDurationCodeType"/>.
		/// </summary>
		public SecondChanceOfferDurationCodeType ListingDuration
		{ 
			get { return ApiRequest.ListingDuration; }
			set { ApiRequest.ListingDuration = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.ItemID"/> of type <see cref="string"/>.
		/// </summary>
		public string ItemID
		{ 
			get { return ApiRequest.ItemID; }
			set { ApiRequest.ItemID = value; }
		}
		
 		/// <summary>
		/// Gets or sets the <see cref="AddTransactionConfirmationItemRequestType.Comments"/> of type <see cref="string"/>.
		/// </summary>
		public string Comments
		{ 
			get { return ApiRequest.Comments; }
			set { ApiRequest.Comments = value; }
		}
		
		
 		/// <summary>
		/// Gets the returned <see cref="AddTransactionConfirmationItemResponseType.ItemID"/> of type <see cref="string"/>.
		/// </summary>
		public string ItemIDResult
		{ 
			get { return ApiResponse.ItemID; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddTransactionConfirmationItemResponseType.StartTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime StartTime
		{ 
			get { return ApiResponse.StartTime; }
		}
		
 		/// <summary>
		/// Gets the returned <see cref="AddTransactionConfirmationItemResponseType.EndTime"/> of type <see cref="DateTime"/>.
		/// </summary>
		public DateTime EndTime
		{ 
			get { return ApiResponse.EndTime; }
		}
		

		#endregion

		
	}
}
