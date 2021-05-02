# XNodePluginOrder
A data-oriented sorting order plugin for xNode.
Change paths and add-node menu order without changing sourcecode.

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
