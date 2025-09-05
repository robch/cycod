using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Extension methods for ChatCommand's function call approval.
/// </summary>
public static class ChatCommandApprovalExtensions
    {
        /// <summary>
        /// Checks if a tool or function should be auto-approved or auto-denied.
        /// </summary>
        /// <param name="factory">The CustomToolFunctionFactory.</param>
        /// <param name="name">The name of the function or tool.</param>
        /// <param name="approvedFunctionCallNames">The list of approved function names.</param>
        /// <param name="deniedFunctionCallNames">The list of denied function names.</param>
        /// <returns>
        /// True if the function should be auto-approved,
        /// False if the function should be auto-denied,
        /// Null if user input is required.
        /// </returns>
        public static bool? ShouldAutoApproveOrDeny(
            this CustomToolFunctionFactory factory,
            string name,
            HashSet<string> approvedFunctionCallNames,
            HashSet<string> deniedFunctionCallNames)
        {
            // Already approved/denied in this session
            if (approvedFunctionCallNames.Contains(name))
                return true;
            
            if (deniedFunctionCallNames.Contains(name))
                return false;
                
            // Check auto-approve settings
            if (factory.ShouldAutoApproveTool(name, approvedFunctionCallNames))
                return true;
                
            // Check auto-deny settings
            if (factory.ShouldAutoDenyTool(name, deniedFunctionCallNames))
                return false;
                
            // Require user input
            return null;
        }
    }