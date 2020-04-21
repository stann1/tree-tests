DECLARE @TopLevel INT = 1083    -- Top level manager (node)
	    ,@NumLevels   INT = 25;   -- Depth of the hiearchy to traverse
	
	WITH HierarchyTraaversal AS
	(
	    -- rCTE anchor: retrieve the top level node
	    SELECT [Level]=1, ParentId, Id
	        ,NodePath=CAST(ParentId AS VARCHAR(8000)) + '/' + CAST(Id AS VARCHAR(8000))
	    FROM Account
	    WHERE ParentId = @TopLevel
	     
	    UNION ALL
	    
	    -- rCTE recursion: retrieve the following nodes
	    SELECT [Level]+1, a.ParentId, a.Id
	        ,NodePath=NodePath + '/' + CAST(a.Id AS VARCHAR(8000))
	    FROM Account a
	    JOIN HierarchyTraaversal b ON b.Id = a.ParentId
	    WHERE [Level] < @NumLevels --+ 1
	)
	SELECT [Level], ParentId, Id, NodePath
	FROM HierarchyTraaversal
	ORDER BY [Level], ParentId, Id;