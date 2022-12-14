import sys
from py3dbp import Packer, Bin, Item
from mpl_toolkits.mplot3d import Axes3D
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import random

path = sys.argv[1]
file_name = sys.argv[2]

orderId = ""
destination = ""
containerType = ""
utilization = ""
container = []
positions = []
sizes = []
colors = []
colorList = ["crimson", "limegreen", "g", "c", "m", "y", "coral", "orange", "skyblue", "pink", "purple"]
itemTypes = []
itemColorMap = {}

file_path = path + "/" + file_name + ".txt"
with open(file_path) as file:
    lines = [line.split() for line in file]
    
for i in range(len(lines)):
    line = lines[i]
    
    if i == 0:
        orderId = line[0]
        destination = line[1]
        containerType = line[2]
        utilization = line[3]
    elif i == 1:
        container = [float(line[0]), float(line[1]), float(line[2])]
    else:
        position = (float(line[0]), float(line[1]), float(line[2]))
        positions.append(position)
    
        size = (float(line[3]), float(line[4]), float(line[5]))
        sizes.append(size)

        itemId = line[6]
        if itemId not in itemColorMap.keys():
            if len(colorList) > 0:
                itemColorMap[itemId] = (colorList.pop(), 0)
            else:
                itemColorMap[itemId] = ("r", 0)
        
        color,amount = itemColorMap[itemId]
        itemColorMap[itemId] = (color, amount+1)
        colors.append(color)


def cuboid_data2(o, size=(1, 1, 1)):
    X = [[[0, 1, 0], [0, 0, 0], [1, 0, 0], [1, 1, 0]],
         [[0, 0, 0], [0, 0, 1], [1, 0, 1], [1, 0, 0]],
         [[1, 0, 1], [1, 0, 0], [1, 1, 0], [1, 1, 1]],
         [[0, 0, 1], [0, 0, 0], [0, 1, 0], [0, 1, 1]],
         [[0, 1, 0], [0, 1, 1], [1, 1, 1], [1, 1, 0]],
         [[0, 1, 1], [0, 0, 1], [1, 0, 1], [1, 1, 1]]]
    X = np.array(X).astype(float)
    for i in range(3):
        X[:, :, i] *= size[i]
    X += np.array(o)
    return X


def plotCubeAt2(positions, sizes=None, colors=None, **kwargs):
    if not isinstance(colors, (list, np.ndarray)): colors = ["C0"] * len(positions)
    if not isinstance(sizes, (list, np.ndarray)): sizes = [(1, 1, 1)] * len(positions)
    g = []
    for p, s, c in zip(positions, sizes, colors):
        g.append(cuboid_data2(p, size=s))
    return Poly3DCollection(np.concatenate(g), facecolors=np.repeat(colors, 6), **kwargs)


fig = plt.figure(figsize=plt.figaspect(1) * 3)
ax = fig.add_subplot(projection='3d')

pc = plotCubeAt2(positions, sizes, colors, edgecolor="k")
ax.add_collection3d(pc)

ax.set_xlim3d([0, container[0]])
ax.set_ylim3d([container[1], 0])
ax.set_zlim3d([0, container[2]])

"""                                                                                                                                                    
Scaling is done from here...                                                                                                                           
"""
x_scale = container[0]
y_scale = container[1]
z_scale = container[2]

scale = np.diag([x_scale, y_scale, z_scale, 1.0])
scale = scale * (1.0 / scale.max())
scale[3, 3] = 1.0


def short_proj():
    return np.dot(Axes3D.get_proj(ax), scale)


ax.get_proj = short_proj
"""                                                                                                                                                    
to here                                                                                                                                                
"""

legend_patches = []
# order_patch = mpatches.Patch(color="black", label=f'Order No: {orderId}')
# legend_patches.append(order_patch)
# destination_patch = mpatches.Patch(color="black", label=f'Destination: {destination}')
# legend_patches.append(destination_patch)
# container_patch = mpatches.Patch(color="black", label=f'Container Type: {containerType}')
# legend_patches.append(container_patch)

for itemId in itemColorMap.keys():
    color, amount = itemColorMap[itemId]
    patch = mpatches.Patch(color=color, label=f'SKU: {itemId} - Amount: {amount}')
    legend_patches.append(patch)

title = f'Packing Result\nOrder: {orderId}\nDestination: {destination}\nContainer Type: {containerType}\nUtilization: {utilization}%' 
leg = plt.legend(handles=legend_patches, title=title)

png_path = path + "/" + file_name + ".png"
plt.savefig(png_path, bbox_inches='tight')
plt.show()
