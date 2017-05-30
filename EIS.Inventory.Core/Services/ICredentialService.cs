using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface ICredentialService : IDisposable
    {
        /// <summary>
        /// Get the list of credentials
        /// </summary>
        /// <returns></returns>
        IEnumerable<CredentialDto> GetCredentials();

        /// <summary>
        /// Get the list of credentials asssigned to the specified company id
        /// </summary>
        /// <param name="companyId">The id of company</param>
        /// <returns></returns>
        IEnumerable<CredentialDto> GetCredentialsByCompany(int companyId);

        /// <summary>
        /// Get the credentials with the specified id
        /// </summary>
        /// <param name="id">The id of the marketplace</param>
        /// <returns></returns>
        CredentialDto GetCredential(int id);

        /// <summary>
        /// Get the default credential with the specified mode
        /// </summary>
        /// <param name="crendentialType">The type of credential</param>
        /// <param name="mode">Either LIVE or TEST mode</param>
        /// <returns></returns>
        CredentialDto GetCredential(string crendentialType, string mode);

        /// <summary>
        /// Get the credential for the what type and company
        /// </summary>
        /// <param name="crendentialType">The type of crendential</param>
        /// <param name="companyId">The id of the company</param>
        /// <param name="mode">Either TEST or LIVE mode</param>
        /// <returns></returns>
        CredentialDto GetCredential(string crendentialType, int companyId, string mode);

        /// <summary>
        /// Save the credential to the database
        /// </summary>
        /// <param name="credentialModel">The object to save</param>
        /// <returns></returns>
        bool CreateCredential(CredentialDto credentialModel);

        /// <summary>
        /// Update the credential
        /// </summary>
        /// <param name="id">The id of marketplace credential to update</param>
        /// <param name="crendentialModel">The updated object</param>
        /// <returns></returns>
        bool UpdateCredential(int id, CredentialDto crendentialModel);

        /// <summary>
        /// Delete the credential with the specified id
        /// </summary>
        /// <param name="id">The id of marketplace credential to delete</param>
        /// <returns></returns>
        bool DeleteCredential(int id);
    }
}
