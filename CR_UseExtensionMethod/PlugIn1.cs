using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;
using DevExpress.Refactor;

namespace CR_UseExtensionMethod
{
    public partial class PlugIn1 : StandardPlugIn
    {
        // DXCore-generated code...
        #region InitializePlugIn
        public override void InitializePlugIn()
        {
            base.InitializePlugIn();
            registerUseExtensionMethod();
            registerUseExtensionMethodCP();
        }
        #endregion
        #region FinalizePlugIn
        public override void FinalizePlugIn()
        {
            //
            // TODO: Add your finalization code here.
            //

            base.FinalizePlugIn();
        }
        #endregion

        private void registerUseExtensionMethod()
        {
            DevExpress.Refactor.Core.RefactoringProvider UseExtensionMethod = new DevExpress.Refactor.Core.RefactoringProvider(components);
            ((System.ComponentModel.ISupportInitialize)(UseExtensionMethod)).BeginInit();
            UseExtensionMethod.ProviderName = "UseExtensionMethod"; // Should be Unique
            UseExtensionMethod.DisplayName = "Use Extension Method";
            UseExtensionMethod.CheckAvailability += UseExtensionMethod_CheckAvailability;
            UseExtensionMethod.Apply += UseExtensionMethod_Apply;
            ((System.ComponentModel.ISupportInitialize)(UseExtensionMethod)).EndInit();
        }
        private void UseExtensionMethod_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
        {
            // On Method Call
            MethodReferenceExpression MethodReference = ea.Element as MethodReferenceExpression;

            if (MethodReference == null)
                return; // Method does not exist;

            Method MethodDeclaration = (Method)MethodReference.GetDeclaration();
            if (MethodDeclaration == null)
                return; // Method not declared

            // Existing Method is recognised as Extension Method
            if (!CodeRush.Language.IsExtensionMethod(MethodDeclaration))
                return;

            // Method IS NOT CALLED with Namespace qualified
            if (QualifiedWithNamespace(MethodReference))
                return;
            ea.Available = true;
        }
        private void UseExtensionMethod_Apply(Object sender, ApplyContentEventArgs ea)
        {
            // The method in question is assumed to already be in scope.

            // Rewrite call from 
            // ... Method(Param1, OtherParams)
            // to 
            // ...Param1.Method(OtherParams)

            ConvertToExtensionMethodCall(ea.Element);
        }

        private void registerUseExtensionMethodCP()
        {
            DevExpress.CodeRush.Core.CodeProvider UseExtensionMethodCP = new DevExpress.CodeRush.Core.CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(UseExtensionMethodCP)).BeginInit();
            UseExtensionMethodCP.ProviderName = "UseExtensionMethodCP"; // Should be Unique
            UseExtensionMethodCP.DisplayName = "Use Extension Method";
            UseExtensionMethodCP.CheckAvailability += UseExtensionMethodCP_CheckAvailability;
            UseExtensionMethodCP.Apply += UseExtensionMethodCP_Apply;
            ((System.ComponentModel.ISupportInitialize)(UseExtensionMethodCP)).EndInit();
        }
        private void UseExtensionMethodCP_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
        {
            // On Method Call
            MethodReferenceExpression MethodReference = ea.Element as MethodReferenceExpression;

            if (MethodReference == null)
                return; // Method does not exist;

            Method MethodDeclaration = (Method)MethodReference.GetDeclaration();
            if (MethodDeclaration == null)
                return; // Method not declared

            // Existing Method is recognised as Extension Method
            if (!CodeRush.Language.IsExtensionMethod(MethodDeclaration))
                return;

            // Method IS CALLED with Namespace qualified
            if (!QualifiedWithNamespace(MethodReference))
                return;

            ea.Available = true; // Change this to return true, only when your Code should be available.
        }
        private void UseExtensionMethodCP_Apply(Object sender, ApplyContentEventArgs ea)
        {
            // Convert
            // ... SomeNamespace.Method(Param1, OtherParams)
            // to 
            // using SomeNamespace
            // ......
            // ...Param1.Method(OtherParams)

            AddReferenceToNewNamespace(GetNamespaceReference(ea.Element), ea.TextDocument);
            
            ConvertToExtensionMethodCall(ea.Element);
        }

        private bool QualifiedWithNamespace(MethodReferenceExpression methodReference)
        {
            LanguageElement ClassReference = (LanguageElement)methodReference.Nodes[0];
            return (ClassReference.Nodes.Count > 0);
        }
        private ElementReferenceExpression GetNamespaceReference(LanguageElement element)
        {
            MethodReferenceExpression methodReference = element as MethodReferenceExpression;
            MethodCall methodCall = methodReference.Parent as MethodCall;
            ElementReferenceExpression classReference = methodReference.Nodes[0] as ElementReferenceExpression;
            ElementReferenceExpression namespaceReference = classReference.Nodes[0] as ElementReferenceExpression;
            return namespaceReference;
        }
        private void AddReferenceToNewNamespace(ElementReferenceExpression namespaceReferenceExpression, TextDocument Doc)
        {
            var NamespaceReference = new NamespaceReference(namespaceReferenceExpression.FullSignature);
            var Code = CodeRush.CodeMod.GenerateCode(NamespaceReference, false);
            var LastNamespaceReference = CodeRush.Refactoring.FindAllNamespaceReferences(Doc.FileNode).LastOrDefault();
            if (LastNamespaceReference != null)
            {
                Doc.QueueInsert(LastNamespaceReference.Range.Start.OffsetPoint(1, 0), Code);
            }
            else
            {
                var FirstNamespace = Doc.FileNode.FindChildByElementType(LanguageElementType.Namespace);
                Doc.QueueInsert(FirstNamespace.Range.Start, Code);
            }
        }
        private void ConvertToExtensionMethodCall(LanguageElement element)
        {
            MethodReferenceExpression methodReference = element as MethodReferenceExpression;
            MethodCall methodCall = methodReference.Parent as MethodCall;
            SourceRange methodCallRange = methodCall.Range; //Range to overwrite with new code

            ElementReferenceExpression classReference = methodReference.Nodes[0] as ElementReferenceExpression;
            SourceRange classReferenceRange = classReference.Range;

            var firstParam = methodCall.Arguments[0];
            methodCall.Arguments.Remove(firstParam);

            //Rewrite MethodCall
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            ActiveDoc.QueueReplace(methodCallRange, CodeRush.Language.GenerateElement(methodCall));

            // Overwrite ClassQualifier with FirstParam
            ActiveDoc.QueueReplace(classReferenceRange, CodeRush.Language.GenerateElement(firstParam));
            ActiveDoc.ApplyQueuedEdits("Referenced Extension Method");
        }
    }
}