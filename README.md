# XNodePluginOrder
A data-oriented sorting order plugin for xNode.<br>
Change paths and add-node menu order without changing sourcecode.
![Example](https://github.com/nostek/XNodePluginOrder/blob/main/Order.png)

### Installation:
In your CustomNodeGraphEditor cs file override these functions.

		public override string GetNodeMenuName(Type type)
		{
			return XNodeEditor.XNodeOrder.GetNodeMenuName(type, base.GetNodeMenuName(type));
		}

		public override int GetNodeMenuOrder(Type type)
		{
			return XNodeEditor.XNodeOrder.GetNodeMenuOrder(type, base.GetNodeMenuOrder(type));
		}
    
They can be used one by themself or both at the same time.

### Usage:
Create a Order ScriptableObject by rightclicking the project view and selecting Create/xNode/Order.<br>
Sort nodes by draging them up and down the list.<br>
Paths can be changed in the edit field. Clear the field to restore the node default value.
