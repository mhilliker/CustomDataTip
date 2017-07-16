﻿using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace CustomDataTip
{
    /// <summary>
    /// Ignore this class.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType("text")]
    internal sealed class VsTextViewListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            try
            {
                var adornment = textView.Properties.GetProperty<CustomDataTip>(typeof(CustomDataTip));

                textView.Properties.GetOrCreateSingletonProperty(
                    () => new TypeCharFilter(textViewAdapter, textView, adornment));
            }
            catch { }
        }
    }


    internal sealed class TypeCharFilter : IOleCommandTarget
    {
        IOleCommandTarget nextCommandHandler;
        CustomDataTip adornment;
        ITextView textView;
        internal int typedChars { get; set; }

        /// <summary>
        /// Add this filter to the chain of Command Filters
        /// </summary>
        internal TypeCharFilter(IVsTextView adapter, ITextView textView, CustomDataTip adornment)
        {
            this.textView = textView;
            this.adornment = adornment;

            adapter.AddCommandFilter(this, out nextCommandHandler);
        }

        /// <summary>
        /// Get user input and update Typing Speed meter. Also provides public access to
        /// IOleCommandTarget.Exec() function
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int hr = VSConstants.S_OK;
     
            char typedChar;
            if (TryGetTypedChar(pguidCmdGroup, nCmdID, pvaIn, out typedChar))
            {
                adornment.UpdateBar(typedChars++);
            }

            hr = nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            return hr;
        }

        /// <summary>
        /// Public access to IOleCommandTarget.QueryStatus() function
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        /// <summary>
        /// Try to get the keypress value. Returns 0 if attempt fails
        /// </summary>
        /// <param name="typedChar">Outputs the value of the typed char</param>
        /// <returns>Boolean reporting success or failure of operation</returns>
        bool TryGetTypedChar(Guid cmdGroup, uint nCmdID, IntPtr pvaIn, out char typedChar)
        {
            typedChar = char.MinValue;

            if (cmdGroup != VSConstants.VSStd2K || nCmdID != (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
                return false;

            typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            return true;
        }

    }
}
