/*
* Prologue
Created By: Isabel Loney
Date Created: 4/2/2025
Last Revised By: Isabel Loney
Date Revised: 4/2/2025
Purpose: Provides utility methods for creating claims

Preconditions: valid, non-null parameters provided
Postconditions: new claims for user
Error and exceptions: N/A
Invariants: generated claims contain provided values
Other faults: N/A
*/

using System.Security.Claims;

namespace CS_Project_Manager.Utilities
{
    public class ClaimsHelper
    {
        public static List<Claim> GenerateClaims(string firstName, string email)
        {
            return
            [
                new(ClaimTypes.Name, firstName),
                new(ClaimTypes.Email, email),
            ];
        }
    }
}
