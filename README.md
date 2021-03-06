<img src="TotalAI/Editor/Images/TotalAILogo.png" width="200"/> 

# Total AI

<b>In Alpha so expect bugs/issues and breaking changes.  TAI should work on any recent version of Unity.</b>

<a href="http://totalai.org/doc-introduction.html">Documentation</a><br>
<a href="https://www.youtube.com/channel/UCTznMlxoaeJMPm1dC26HvUA">YouTube Channel</a><br>
<a href="https://discord.gg/jf52tnUFX2">Discord Server</a><br>

<a href="https://github.com/TotalAI/TotalAI-3D-Demo">3D Demo World Repo</a><br>
<a href="https://github.com/TotalAI/TotalAI-2D-Demo">2D Demo World Repo</a><br>

Watch the <a href="https://www.youtube.com/watch?v=0iYzfrM0cjI">4 minute Overview Video</a>.

Total AI (TAI) is a complete free open source agent based AI Framework for Unity.
Its goals are to provide an easy to prototype, flexible, fully customizable, and performant framework
for a broad array of AI types and for a broad array of game types.  Eventually TAI hopes to have a
vast library of community created types that anyone can use to jumpstart their AI.

The core of TAI are Agents who can to sense the world, create memories, plans, and act in order to
reduce their Drives based on the Mapping (the core unit of planning) with the most utility. This is accomplished
with its Type System and Plan/Mapping System. It is also easy to extend and customize through ScriptableObject's
pluggable data and logic ability. This makes writing and plugging in your own custom AI logic easy.

## Install & Setup
Read the <a href="http://totalai.org/doc-installation.html">Documentation</a>
or
watch the <a href="https://www.youtube.com/watch?v=XsqUmfCPf7M">Video</a>.

## Features
* Designed from the ground up to be take advantage of Unity's unique features.
* Customizable - Usually will just need to implement a few methods to create a new type.
* Quick to Prototype - easy to switch between various AI types.
* Flexible - ScriptableObject Type system allows for lego-like building parts.
* Multi-Agent - Agent Events allow for multi-agent coordination.
* Intelligent division of Agent’s functions - Planner, Decider, Sensors, Memory, Movement, and Animation.
* World Objects have states, state transitions, grow, built, damage, change appearance, and inventory recipes.
* Inventory - Slot based inventory system that allows any Entity to be in any other Entity’s inventory.
* Factions - Groups of Agents, can be figured into the Planning.
* Drive (Motivation) based Planning - Agents will choose most pressing drive to lower.
* Complex logic for Drive level changes - faction synced - attribute synced - custom equation changes.
* Target Factors - Utility AI like selection process for choosing best target and best inventory target.
* Selectors - Advanced selection logic for choosing values to use in a Mapping.
* Mapping - Core unit of planning has Target Factors, Utility Modifiers, Input Conditions, and Output Changes.
* Behavior/Decider Logic - Allows for complex logic for running and interrupting Mappings.
* Mulitple supported AI types - GOAP, Utility AI, and FSM - Deep RL using ml-agents coming soon.
* Attributes - Generic value types for Agents.
* Roles - Allows for Actions and Drives to be changed dynamically.
* Tags - Useful for controlling Mappings and can also be used to create relationships between Entities.
* TypeCategories/TypeGroups - Attach multiple categories to Entities - Generalizes Mappings.
* Agent View Editor - Agent properties, realtime history logging and plan tree visualization.
* Setup Editor - Quick Basic Setup for a Project using the Setup Editor.
* Custom Inspectors - Extensive use of custom inspectors for increased ease of use.
* Extensive Documentation with goal of having 100% documentation coverage.

## Agent Logic Flow Diagram
<img src="AgentFlow.png" /> 

## Contribute
Are you passionate about AI and Unity?  Come apply your passion to help make Total AI great!
This is the perfect time to jump in and have a major impact on the current and future development of Total AI.

See <a href="http://totalai.org/contrib-introduction.html">Contribute Documentation</a> for more Information.

## Financial Support
Do you want to see continued progress on Total AI's push from Alpha to Production?
Consider financial support for the creator of TAI on <a href="https://patreon.com/zacwarren">Patreon</a>.
All Patrons of $5 or more will have the option to be featured on this Readme and
The website's <a href="http://totalai.org/contributors.html">Contributors Page</a>
along with a logo and a link to their personal/business website.

## Currently Implemented AI Types
* <a href="http://totalai.org/doc-goap.html">GOAP</a>
* <a href="http://totalai.org/doc-utility-ai.html">Utility AI</a>
* <a href="http://totalai.org/doc-fsm.html">Finite State Machines</a>

## In Planning/Development
* <a href="http://totalai.org/doc-deep-rl.html">Deep RL</a> - using <a href="https://github.com/Unity-Technologies/ml-agents">ml-agents</a>

## Integrations
Due to Total AI's comprehesive Type System intregrating your Asset with Total AI should be simple.
For example if your Asset is agent Sensors, you will just need to create custom
<a href="http://totalai.org/doc-sensortype.html">Sensor Types</a> that call your Sensors.

Let us know if you have an integration in your repo and/or would like us to create a separate repo for your TAI integration.

## Type Packs
Did you create a set of Types for a certain style of game and want to contribute?  Consider making a Type Pack so
others can use it to quick start their game.  Let us know and we'll add a repo for it.

## Add-Ons
Did you create a set of Types for a certain Unity feature?  For example a type system for Unity's Animation Rigging System.
Consider turning it into an Add-On so others can use it.  Let us know and we'll add a repo for it.

## MIT Licensed
A short, permissive software license. Basically, you can do whatever you want as long as you include the original copyright
and license notice in any copy of the software/source.
