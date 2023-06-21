# Introduction

The pattern control technique has emerged as a promising approach for au-
tomating the verification of computer systems. Among the various formalisms
used in software engineering, Petri Nets (PDR) hold a significant position. How-
ever, the existing modeling and analysis tools for Petri Nets often lack effective
verification techniques. Many of these tools are either outdated or offer limited
verification capabilities.
In this mini-project, we aim to address these challenges by leveraging the
power of the NuSMV tool [1] to verify Petri net models. Our objective is two-
fold: first, we focus on constructing the graphical representation of a Petri net,
known as a marking graph. Then, in the subsequent step, we transform the
obtained marking graph into an equivalent NuSMV code.
The algorithm for converting the marking graph of a Reachability Digraph
Petri Net (RDP) into NuSMV code is depicted in Figure 1. This algorithm serves
as the core foundation of our approach. To provide a practical understanding of
its implementation, we present an illustrative example in Figure 2, showcasing
the utilization of the algorithm.
Throughout this report, we will delve into the project’s structure, the libraries
employed, and comprehensively explore the algorithms employed for building
the marking graph and transforming it into NuSMV code. By combining the
strengths of Petri Nets and NuSMV, we aim to enhance the verification capabil-
ities of modeling and analysis tools for computer systems.
In the subsequent sections, we will provide a detailed walkthrough of our ap-
proach, highlighting the key components, design choices, and the overall func-
tionality of our Petri net and NuSMV code generator.


## Used Tools

**ProgrammingLanguage**

C# was chosen as the programming language for developing the code generator.
Its robustness, object-oriented features, and extensive library support make it
suitable for implementing complex algorithms and building scalable software
solutions.

**UIFramework**

The UI of our app was developed using Windows Presentation Foundation (WPF)
with .NET 6. WPF provides a rich set of tools and controls for creating visually
appealing and interactive user interfaces. With the latest version of .NET, we
benefit from improved performance and enhanced capabilities.

**GraphLibrary**

To draw the marking graphs, we utilized the Microsoft.Msagl.GraphViewerGDI
library. This library offers graph layout and visualization features, allowing us
to represent the Petri net models as graphical diagrams. By leveraging this li-
brary, we can provide users with an intuitive visual representation of the Petri
nets and their corresponding markings.

**NuSMVCommandLineTool**

For the verification of generated NuSMV code, we employed the NuSMV com-
mand line tool. NuSMV is a well-established symbolic model checker widely
used for verifying the correctness of systems specified in various formal lan-
guages, including NuSMV’s own modeling language. By utilizing the power
of NuSMV, we can check properties and validate the behavior of the Petri net
models translated into NuSMV code.


## Application functionality and usage


When you first run the app, you will see the parameters window, as shown in
Figure 1. This window allows you to specify:

```
■ The number of places and transitions in the Petri net.
■ Pre and post matrices of the Petri net (separated by a comma).
■ The initial marking of the Petri net.
■ The k-safe property of the Petri net (indicates the maximum number of
tokens that can be present in any place at any given time, it’s used to avoid
crashes when the Petri net is unbounded)
```

![image](/screenshots/Screenshot_1.png)

```
Figure 0.1: Parameters window(the example given by the teacher)
```

After specifying the parameters, you can click on the "Generate" button to
draw the corresponding Pteri net, the Pre arcs are represented by black arrows,
the Post arcs are represented by red arrows, and the initial marking writen in
the places between brackets. The generated Petri net is shown in Figure 2.

![image](/screenshots/Screenshot_2.png)

```
Figure 0.2: Generated Petri net
```
After generating the Petri net, you can click on the "Calculate Marking" but-
ton to calculate the markings and draw the corresponding marking graph. the
calculated markings will be shown in the list box on the right side of the win-
dow, and the marking graph will be shown in the window. The marking graph
for the Petri net in Figure 2 is shown in Figure 3.

![image](/screenshots/Screenshot_3.png)

```
Figure 0.3: Marking graph
```
After generating the marking graph, you can click on the "Generate NuSMV
Code" button to generate the corresponding NuSMV code. The generated code
will be open in a new window. And a second window will be opened to verify


properties using NuSMV. The generated NuSMV code for the Petri net in Figure
2 is shown in Figure 4.

![image](/screenshots/Screenshot_4.png)

```
Figure 0.4: Generated NuSMV code
```
After generating the NuSMV code, you can click on the "Check" button to
verify a property using NuSMV. The "Check" button will be colored in green
if the property is verified, and it will be colored in red if the property is not
verified. The details of the verification will be shown in the text box below the
"Check" button. The result of the verification will be shown in the window. The
result of the verification for the property "G (p0 = TRUE)" is shown in Figure 5.

![image](/screenshots/Screenshot_5.png)

```
Figure 0.5: Result of the verification for the property "G (p0 = TRUE)"
```

## Algorithms

In this section, we will discuss the algorithms used in the implementation of our
project.

**Petri net drawing algorithm**

The Petri net drawing algorithm in the code provided is responsible for constructing the graphical representation of a Petri net based on the given param-
eters as shown in Figure 6. Here is a detailed description of how the algorithm
works:
Adding Nodes: The algorithm begins by iterating over the number of nodes
specified in the parameters. For each node, a new Node object is created and
added to the graph. The node is labeled with "P" followed by its corresponding
index, representing a place in the Petri net. The node is displayed as a circle
shape.
Creating Pre- and Post-Edges: The algorithm then iterates over each node
and transition combination based on the parameters. For each combination, it
checks the pre and post matrices to determine the connections between places
and transitions in the Petri net.
Pre-Matrix: If the value in the pre-matrix for the current node and transition
combination is not zero, an edge is added from the transition to the place. The
source label of the edge is set to "T" followed by the transition index, and the
target label is set to "P" followed by the place index. The color of the edge is set
to black. If the value in the pre-matrix is not 1, indicating a weight greater than
1, a label displaying the weight value is added to the edge.
Post-Matrix: Similarly, if the value in the post-matrix for the current node
and transition combination is not zero, an edge is added from the place to the
transition. The source label of the edge is set to "P" followed by the place index,
and the target label is set to "T" followed by the transition index. The color of
the edge is set to red. If the value in the post-matrix is not 1, a label displaying
the weight value is added to the edge.
Initial Marking: Finally, the algorithm adds the initial marking for each node.
It iterates over the array of initial markings specified in the parameters. The
marking value is retrieved, and the corresponding node in the graph is found

using the label "P" followed by the place index. The node’s label text is updated
to display the place index along with its marking value in parentheses.
By executing this algorithm, the Petri net drawing functionality of the appli-
cation constructs the graphical representation of the Petri net model, depicting
the places, transitions, and their connections. The resulting visualization pro-
vides a visual representation of the Petri net structure, facilitating the under-
standing and analysis of the modeled system.

![image](/screenshots/code_1.png)

```
Figure 0.6: Petri net drawing algorithm
```

**Marking graph algorithm**

The Marking graph algorithm provided is responsible for calculating the mark-
ings and constructing the marking graph for the Petri net model as shown in
Figure 7. Here is a detailed description of how the algorithm works:
Initialization: The algorithm initializes a dictionary places to store the current
markings of each place and a list markings to store all the encountered mark-
ings. The initial marking is obtained using the GetPlacesFromInitialMarking()
function and added to markings.
The algorithm also initializes a list of transitions transitions obtained using
the GetTransitionsFromMatrices() function, which contains the pre and post ma-
trices.
Reachability Graph Construction: The algorithm constructs the reachability
graph using a breadth-first search approach. It maintains a queue queue to store
the markings to be processed and a set visitedMarkings to keep track of the
visited markings.
It starts by adding the initial marking to the queue and the graph. The current
marking is dequeued, and a node is created in the graph to represent the current
marking.
Applying Transitions: For each transition in transitions, the algorithm ap-
plies the transition to the current marking using the ApplyTransition() function.
The resulting new marking is obtained.
Handling New Markings: If the new marking has not been visited before, it
is added to visitedMarkings, markings, and the queue for further exploration.
A new node is added to the graph to represent the new marking, and an edge
is created between the current node and the new node, labeled with the corre-
sponding transition.
If the new marking has been visited before, it means that the Petri net is
bounded. The algorithm checks if an edge already exists between the current
node and the existing node representing the new marking. If it does, the tran-
sition label is appended to the existing edge. Otherwise, a new edge is added
to the graph between the current node and the existing node, labeled with the
corresponding transition.
Boundedness Check: After processing the transitions, the algorithm checks
if the marking in any place exceeds the maximum value MAX K. If any place’s


marking exceeds MAX K, it indicates that the Petri net is not bounded. A warn-
ing message is displayed, and the algorithm terminates.
By executing this algorithm, the marking graph is constructed, representing
the different markings and their transitions in the Petri net model. The resulting
graph provides insights into the reachability and boundedness of the Petri net,
facilitating the analysis and verification of system behavior.

![image](/screenshots/code_2.png)

```
Figure 0.7: Marking graph algorithm
```

**NuSMV code generation algorithm**

The algorithm provided generates the nuSMV code based on the given Petri net
model as shown in Figure 8. Here is a detailed description of how the algorithm
works:
Initialization: The algorithm initializes a string variable smvCode with the
initial nuSMV code structure.
Define the States: The algorithm adds a declaration for the s variable repre-
senting the states of the Petri net model. It uses the markings array to determine
the number of states and adds them to the s variable declaration.
Define Variables for Places: For each place in the Petri net model, the algo-
rithm adds a variable declaration to the nuSMV code. If the place is boolean, it
declares a boolean variable pX. Otherwise, it declares a bounded integer variable
pX ranging from 0 to the maximum marking value for that place.
Initialization and Transition Relations: The algorithm continues by adding
the initialization and transition relation parts of the nuSMV code. It initializes
the s variable to s0 and defines the next state based on the current state.
Transition Relation Cases: For each state in the Petri net model, the algorithm
generates a transition relation case. It determines the successors of the current
state and adds them as possible transitions in the nuSMV code.
Labelling Functions: Next, the algorithm generates the labelling functions
for each place in the Petri net model. It creates a switch statement for each place
and assigns the corresponding markings based on the current state.
For boolean places, it sets the value to TRUE or FALSE depending on the
marking. For non-boolean places, it assigns the corresponding marking value.
Default Values: The algorithm sets default values for each labelling function
based on the variable type. For boolean places, the default value is set to FALSE,
and for non-boolean places, the default value is set to 0.
Finalizing the Code: The algorithm closes the labelling function switch state-
ments and returns the generated nuSMV code.

![image](/screenshots/code_3.png)

```
Figure 0.8: NuSMV code generation algorithm
```


## Requirements

- Visual Studio 2019 or later
- .NET 6.0 or later
- NuSMV 2.6.0 or later


## License
This project is licensed under the GPL-2.0 license - see the  [LICENSE.txt](LICENSE.txt) file for details.

 



