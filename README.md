CR_UseExtensionMethod
=====================

UseExtensionMethod provides a Refactoring and a CodeProvider.

These are designed to operate on method calls. They are available on any method call who's method can be called as an extension method.

**Example 1:**

Given a call top an Extension Method (EM) in a class (C) which passes a parameter P...

    C.EM(P);

...the code will be rewritten as...

    P.E();

The Refactoring will be made available in cases, as above, where the call to the method is missing any relevant Namespace.

**Example 2:**

In cases where a namespace (N) is present...

    N.C.EM(P);

...the code will be rewritten exactly as before...

    P.E();

However a using\imports directive will also be added to the code file.
This addition may cause the importing of additional classes and methods which might change the meaning of some code. For this reason, this version of the functionality is made available through a CodeProvider rather than a Refactoring.
****
