<h1>Offsetter</h1>

A Windows application for generating 2D Uniform and Non-Uniform Offset chains (a chain being a closed set of contiguous lines and/or arcs). The input and output are AutoCAD DXF files, having potential use in Computer Aided Machining (CAM) and obstacle avoidance applications.

<h2>Details</h2>

The Uniform Offsetting implementation (roughly) follows the algorithm described by Hansen & Arbab and for Non-Uniform Offsetting (a.k.a. Minkowski Sum) the implementation follows the algorithm described by Ramkumar.

Whereas the Minkowski Sum is typically applied to convex shapes this implementation also supports (limited) concave shapes. Shapes having concavities greatly increase complexity and (as implemented) fails in some cases. Some failures can be seen using the test data nest15a, nest15b and nest16a.

Currently, all input must be DXF profile entities. Non-Uniform Offsetting requires at least two profiles, one representing a part/shape and the other a tool for traversing the shape outline. The tool must be centered at the origin (0,0). When submitting files to automated/batch testing of Non-Uniform Offsetting, the first profile must represent the shape and the second the tool.

Debugging output is configured via the file data\config.json.

Test data reside in TestData.zip (as opposed to individual repository files) to limit repository size/pollution.

<h2>References</h2>

Hansen,A and Arbab,F An Algorithm for Generating NC Tool Paths for Arbitrarily Shaped Pockets with Islands. ACM Transactions on Graphics (TOG), Volume 11, Issue 2
(April 1992).

Ramkumar,G.D. An Algorithm to Compute the Minkowski Sum Outer-face of Two Simple Polygons. SGC'96:Proceedings of the twelfth annual symposium on Computational geometry
