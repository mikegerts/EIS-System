using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Resources;
using System.Runtime.InteropServices;
using MSXML2;

using eBay.Service.Core.Soap;
using eBay.Service.Core.Sdk;
using eBay.Service.Call;
using eBay.Service.Util;
using eBay.Service.SDK.Util;
namespace eBay.Service.SDK.Attribute
{
	/// <summary>
	/// Defines object to assist developers to easily integrate eBay attributes feature.
	/// </summary>
	public interface IAttributesMaster
	{
		/// <summary>
		/// The Attributes XSL provider.
		/// </summary>
		IAttributesXslProvider XslProvider
		{ get; set; }

		/// <summary>
		/// The Attributes XML provider.
		/// </summary>
		IAttributesXmlProvider XmlProvider
		{ get; set; }

		/// <summary>
		/// The Attributes CategoryCS provider. You only need to set this property if you
		/// are going to call <c>RenderHtmlForCategories</c> method.
		/// </summary>
		ICategoryCSProvider CategoryCSProvider
		{ get; set; }

		/// <summary>
		/// Extract <c>IAttributeSetCollection</c> object from <c>IKeyValueCollection</c> object.
		/// </summary>
		/// <param name="nameValues">The <c>IKeyValueCollection</c> to be converted.</param>
		/// <returns>The extracted <c>IAttributeSetCollection</c> object.</returns>
		IAttributeSetCollection NameValuesToAttributeSets(IKeyValueCollection nameValues);

		/// <summary>
		/// Render HTML text by specifying list of AttributeSet. 
		/// </summary>
		/// <param name="attrSets">List of AttributeSet objects.</param>
		/// <param name="errorList">The <c>IErrorSetCollection</c> object returned by <c>Validate</c> method.
		/// Set to null if you don't have one.</param>
		/// <returns>The generated HTML text that is encapsulated in HTML table element.</returns>
		string RenderHtml(IAttributeSetCollection attrSets, IErrorSetCollection errorList);

		/// <summary>
		/// Render HTML text by specifying list of AttributeSet and xsl Document. 
		/// </summary>
		/// <param name="attrSets">List of AttributeSet objects.</param>
		/// <param name="xslDoc">Xsl Document</param>
		/// <param name="errorList">The <c>IErrorSetCollection</c> object returned by <c>Validate</c> method.
		/// Set to null if you don't have one.</param>
		/// <returns>The generated HTML text that is encapsulated in HTML table element.</returns>
		string RenderHtml(IAttributeSetCollection attrSets, DOMDocument30 xslDoc, IErrorSetCollection errorList); 

		/// <summary>
		/// Render HTML text by raw name-value pairs that you got during HTML submit. 
		/// </summary>
		/// <param name="nameValues">List of name-value pairs from submit of attributes HTML form
		/// generated by all these RenderHtml methods.</param>
		/// <param name="errorList">The <c>IErrorSetCollection</c> object returned by <c>Validate</c> method.
		/// Set null if you don't have one.</param>
		/// <returns>The generated HTML text that is encapsulated in HTML table element.</returns>
		string RenderHtmlForPostback(IKeyValueCollection nameValues, IErrorSetCollection errorList);

		/// <summary>
		/// Validate <c>IAttributeSetCollection</c> object against eBay Attributes rules.
		/// IErrorSetCollection.Count == 0 means validation succeeded. Otherwise means failure and you
		/// have to call the above RenderHtml... methods and pass in the <c>IErrorSetCollection</c> object
		/// to re-generate Attributes HTML text that contains all the error messages.
		/// </summary>
		/// <param name="attrSets">The <c>IAttributeSetCollection</c> object which you want to validate.</param>
		/// <returns>The returned <c>IAttributeSetCollection</c> object. IAttributeSetCollection == 0 means 
		/// validation succeeded.</returns>
		IErrorSetCollection Validate(IAttributeSetCollection attrSets);

		/// <summary>
		/// Convert attribute set array to the format of ItemType for AddItemCall.
		/// </summary>
		/// <param name="attrSets">The attribute set list generated by AttributeMaster.</param>
		/// <returns>The converted array that is compatible with ItemType in AddItemCall.</returns>
		AttributeSetTypeCollection ConvertAttributeSetArray(IAttributeSetCollection attrSets);

		/// <summary>
		/// Creates an array of AttributeSet objects which contains item specific attribute sets and
		/// site wide attribute sets with the exception of the attribute set for return policy. 
		/// </summary>
		/// <param name="itemSpecAttrSets">IAttributeSetCollection</param>
		/// <param name="swAttrSets">IAttributeSetCollection</param>
		/// <returns>Joined collection of of item specific attributes sets and site wide attribute sets, except Return Policy</returns>
		IAttributeSetCollection JoinItemSpecificAndSiteWideAttributeSets(IAttributeSetCollection itemSpecAttrSets, IAttributeSetCollection swAttrSets); 

		/// <summary>
		/// Returns collection of item specific AttributeSet objects for an array category Ids.
		/// Each element of the array contains a VCS Id, if it exists for a given category Id. 
		/// </summary>
		/// <param name="catIds">int[]</param>
		/// <returns>Collection of item specific AttributeSet objects</returns>
		IAttributeSetCollection GetItemSpecificAttributeSetsForCategories(Int32Collection catIds); 

		/// <summary>
		/// Extracts AttributeSet object for Return Policy from site wide attribute sets.
		/// </summary>
		/// <param name="siteWideAttrSets">IAttributeSetCollection</param>
		/// <returns>Return Policy AttributeSet object</returns>
		AttributeSet GetReturnPolicyAttributeSet(IAttributeSetCollection siteWideAttrSets); 

		/// <summary>
		/// Returns collection of Site Wide AttributeSet objects for a given array category Ids.
		/// Each element of the array contains a VCS Id, if it exists for a given category Id. 
		/// </summary>
		/// <param name="catIds">int[]</param>
		/// <returns>Collection of site wide AttributeSet objects</returns>
		IAttributeSetCollection GetSiteWideAttributeSetsForCategories(Int32Collection catIds);

	}


}