<h1>Offsetter</h1>

A Windows application for generating 2D Uniform and Non-Uniform Offset chains (a closed set of contiguous lines and/or arcs). The input and output are AutoCAD DXF files, having potential use in Computer Aided Machining (CAM) and obstacle avoidance applications.

<h2>Details</h2>

The Uniform Offsetting implementation (roughly) follows the algorithm described by Hansen & Arbab. Likewise, Non-Uniform Offsetting (a.k.a. Minkowski Sum) implementation follows the algorithm described by Ramkumar.

Whereas the Minkowski Sum is typically applied to convex shapes this implementation also supports (limited) concave shapes. Shapes having concavities greatly increase complexity and fails in some cases.

Currently, all input shapes must be DXF profiles entities. The input used by non-uniform offsetting requires the first profile to represent the part outline and the second the tool outline. The latter must be centered at the origin (0,0).

A variety of test data resides in TestData.zip. Failures of the non-uniform offsetting implementation can be seen using the files nest15a, nest15b and nest16a.

<h2>References</h2>

Hansen,A and Arbab,F An Algorithm for Generating NC Tool Paths for Arbitrarily Shaped Pockets with Islands. ACM Transactions on Graphics (TOG), Volume 11, Issue 2
(April 1992).

Ramkumar,G.D. An Algorithm to Compute the Minkowski Sum Outer-face of Two Simple Polygons. SGC'96:Proceedings of the twelfth annual symposium on Computational geometry
