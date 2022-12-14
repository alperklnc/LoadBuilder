import sys
import pandas as pd
import decimal
from decimal import Decimal


DEFAULT_NUMBER_OF_DECIMALS = 3
START_POSITION = [0, 0, 0]

def get_limit_number_of_decimals(number_of_decimals):
    return Decimal('1.{}'.format('0' * number_of_decimals))

def set_to_decimal(value, number_of_decimals):
    number_of_decimals = get_limit_number_of_decimals(number_of_decimals)

    return Decimal(value).quantize(number_of_decimals)

def rect_intersect(item1, item2, x, y):
    """Estimate whether two items get intersection in one dimension.
    Args:
        item1, item2: any two items in item list.
        x,y: Axis.LENGTH/ Axis.Height/ Axis.WIDTH.
    Returns:
        Boolean variable: False when two items get intersection in one dimension; True when two items do not intersect in one dimension.
    """
    
    d1 = item1.get_dimension() 
    d2 = item2.get_dimension() 
    
    cx1 = item1.position[x] + d1[x]/2 
    cy1 = item1.position[y] + d1[y]/2
    cx2 = item2.position[x] + d2[x]/2 
    cy2 = item2.position[y] + d2[y]/2
    
    ix = max(cx1, cx2) - min(cx1, cx2) # ix: |cx1-cx2|
    iy = max(cy1, cy2) - min(cy1, cy2) # iy: |cy1-cy2|
    
    return ix < (d1[x] + d2[x])/2 and iy < (d1[y] + d2[y])/2 

def intersect(item1, item2):
    """Estimate whether two items get intersection in 3D dimension.
    Args:
        item1, item2: any two items in item list.
    Returns:
        Boolean variable: False when two items get intersection; True when two items do not intersect.
    """
    
    return ( 
    rect_intersect(item1, item2, Axis.LENGTH, Axis.HEIGHT) and # xz dimension
    rect_intersect(item1, item2, Axis.HEIGHT, Axis.WIDTH) and # yz dimension
    rect_intersect(item1, item2, Axis.LENGTH, Axis.WIDTH)) # xy dimension
    
def stack(item1, item2):
    """Stack two items with same length, width, height or any two of three sides are same.
    Args:
        item1, item2: any two items in item list.
    Return:
        item1/ stacked_item_list/ stacked_item.
    """
    
    if (
        item1.length == item2.length and
        item1.width == item2.width and
        item1.height == item2.height
    ):
        stacked_item_1 = Item(item1.name + item2.name, item1.length + item2.length, 
                              item1.width, item1.height, item1.weight + item2.weight) #(2l, w, h)
        stacked_item_2 = Item(item1.name + item2.name, item1.length, item1.width + item2.width, 
                              item1.height, item1.weight + item2.weight) #(l, 2w, h)
        stacked_item_3 = Item(item1.name + item2.name, item1.length, item1.width, 
                              item1.height + item2.height, item1.weight + item2.weight) #(l, w, 2h)
        
        stacked_item_list = [stacked_item_1, stacked_item_2, stacked_item_3]
        
        return stacked_item_list
        
    elif ( 
        item1.length == item2.length and
        item1.width == item2.width and
        item1.height != item2.height
    ):
        stacked_item = Item(item1.name + item2.name, item1.length, item1.width, 
                            item1.height + item2.height, item1.weight + item2.weight) #(l, w, 2h)
        
        return stacked_item
    
    elif (
        item1.length == item2.length and 
        item1.height == item2.height and
        item1.width != item2.width
    ):
        stacked_item = Item(item1.name + item2.name, item1.length, item1.width + item2.width, 
                            item1.height, item1.weight + item2.weight) #(l, 2w, h)
        
        return stacked_item
    
    elif (
        item1.width == item2.width and
        item1.height == item2.height and
        item1.length != item2.length
    ):
        stacked_item = Item(item1.name + item2.name, item1.length + item2.length, 
                            item1.width, item1.height, item1.weight + item2.weight)
        
        return stacked_item #(2l, w, h)
    
    else:
        return item1

# length in x-axis; width in y-axis; height in z-axis
class RotationType:
    RT_LWH = 0
    RT_HLW = 1
    RT_HWL = 2
    RT_WHL = 3
    RT_WLH = 4
    RT_LHW = 5
    
    ALL = [RT_LWH, RT_HLW, RT_HWL, RT_WHL, RT_WLH, RT_LHW]
 
# (x, y, z) --> (length, width, height)
class Axis:
    LENGTH = 0
    WIDTH = 1
    HEIGHT = 2
    
    ALL = [LENGTH, WIDTH, HEIGHT]


class Bin:
    def __init__(self, size, length, width, height, capacity):
        self.size = size 
        self.length = length
        self.width = width
        self.height = height
        self.capacity = capacity
        self.total_items = 0 # number of total items in one bin
        self.items = [] # item in one bin -- a blank list initially
        self.unplaced_items = []
        self.unfitted_items = [] # unfitted item in one bin -- a blank list initially
        self.number_of_decimals = DEFAULT_NUMBER_OF_DECIMALS
    
    def format_numbers(self, number_of_decimals):
        self.length = set_to_decimal(self.length, number_of_decimals)
        self.height = set_to_decimal(self.height, number_of_decimals)
        self.width = set_to_decimal(self.width, number_of_decimals)
        self.capacity = set_to_decimal(self.capacity, number_of_decimals)
        self.number_of_decimals = number_of_decimals
    
    def get_volume(self):
        return set_to_decimal(
            self.length * self.height * self.width, self.number_of_decimals)
    
    def get_size(self):
        return self.size
    
    def get_lenght_bin(self):
        return self.length
    
    def get_width_bin(self):
        return self.width
    
    def get_height_bin(self):
        return self.height
     
    def get_total_weight(self):
        total_weight = 0
        
        for item in self.items:
            total_weight += item.weight
        
        return set_to_decimal(total_weight, self.number_of_decimals)
    
    def get_filling_ratio(self):
        total_filling_volume = 0
        total_filling_ratio = 0
        
        for item in self.items:
            total_filling_volume += item.get_volume()
            
        total_filling_ratio = total_filling_volume / self.get_volume()
        return set_to_decimal(total_filling_ratio, self.number_of_decimals)
    
    def can_hold_item_with_rotation(self, item, pivot): 
        """Evaluate whether one item can be placed into bin with all optional orientations.
        Args:
            item: any item in item list.
            pivot: an (x, y, z) coordinate, the back-lower-left corner of the item will be placed at the pivot.
        Returns:
            a list containing all optional orientations. If not, return an empty list.
        """
        
        fit = False 
        valid_item_position = [0, 0, 0]
        item.position = pivot 
        rotation_type_list = [] 
        

        rotation_types = []
        if item.is_rotation_allowed:
            rotation_types = RotationType.ALL     
        else:
            rotation_types = [0,4]
            
        
        for i in rotation_types: 
            item.rotation_type = i
            dimension = item.get_dimension() 
            if (
                pivot[0] + dimension[0] <= self.length and  # x-axis
                pivot[1] + dimension[1] <= self.width and  # y-axis
                pivot[2] + dimension[2] <= self.height     # z-axis
            ):
            
                fit = True
                
                for current_item_in_bin in self.items: 
                    if intersect(current_item_in_bin, item): 
                        fit = False
                        item.position = [0, 0, 0]
                        break 
                
                if fit: 
                    if self.get_total_weight() + item.weight > self.capacity: # estimate whether bin reaches its capacity
                        fit = False
                        item.position = [0, 0, 0]
                        continue 
                    
                    else: 
                        rotation_type_list.append(item.rotation_type) 
            
            else:
                continue 
        
        return rotation_type_list 

    def put_item(self, item, pivot, distance_3d): 
        """Evaluate whether an item can be placed into a certain bin with which orientation. If yes, perform put action.
        Args:
            item: any item in item list.
            pivot: an (x, y, z) coordinate, the back-lower-left corner of the item will be placed at the pivot.
            distance_3d: a 3D parameter to determine which orientation should be chosen.
        Returns:
            Boolean variable: False when an item couldn't be placed into the bin; True when an item could be placed and perform put action.
        """
        
        fit = False 
        rotation_type_list = self.can_hold_item_with_rotation(item, pivot)
        margins_3d_list = []
        margins_3d_list_temp = []
        margin_3d = []
        margin_2d = []
        margin_1d = []
        
        n = 0
        m = 0
        p = 0
        
        if not rotation_type_list:
            return fit 
        
        else:
            fit = True 
            
            rotation_type_number = len(rotation_type_list)
            
            if rotation_type_number == 1: 
                item.rotation_type = rotation_type_list[0] 
                self.items.append(item)
                self.total_items += 1
                return fit 
            
            else: 
                for rotation in rotation_type_list: 
                    item.rotation_type = rotation
                    dimension = item.get_dimension()
                    margins_3d = [distance_3d[0] - dimension[0], 
                                 distance_3d[1] - dimension[1], 
                                 distance_3d[2] - dimension[2]]
                    margins_3d_temp = sorted(margins_3d)
                    margins_3d_list.append(margins_3d)
                    margins_3d_list_temp.append(margins_3d_temp)
                
                while p < len(margins_3d_list_temp):
                    margin_3d.append(margins_3d_list_temp[p][0])
                    p += 1
                
                p = 0
                while p < len(margins_3d_list_temp):
                    if margins_3d_list_temp[p][0] == min(margin_3d):
                        n += 1
                        margin_2d.append(margins_3d_list_temp[p][1])
                    
                    p += 1
                
                if n == 1:
                    p = 0
                    while p < len(margins_3d_list_temp):
                        if margins_3d_list_temp[p][0] == min(margin_3d):
                            item.rotation_type = rotation_type_list[p]
                            self.items.append(item)
                            self.total_items += 1
                            return fit 
                        
                        p += 1
                
                else:
                    p = 0
                    while p < len(margins_3d_list_temp):
                        if (
                            margins_3d_list_temp[p][0] == min(margin_3d) and
                            margins_3d_list_temp[p][1] == min(margin_2d)
                        ):
                            m += 1
                            margin_1d.append(margins_3d_list_temp[p][2])
                        
                        p += 1
                
                if m == 1:
                    p = 0
                    while p < len(margins_3d_list_temp):
                        if (
                            margins_3d_list_temp[p][0] == min(margin_3d) and
                            margins_3d_list_temp[p][1] == min(margin_2d)
                        ):
                            item.rotation_type = rotation_type_list[p]
                            self.items.append(item)
                            self.total_items += 1
                            return fit 
                        
                        p += 1
                
                else:
                    p = 0
                    while p < len(margins_3d_list_temp):
                        if (
                            margins_3d_list_temp[p][0] == min(margin_3d) and
                            margins_3d_list_temp[p][1] == min(margin_2d) and
                            margins_3d_list_temp[p][2] == min(margin_1d)
                        ):
                            item.rotation_type = rotation_type_list[p]
                            self.items.append(item)
                            self.total_items += 1
                            return fit 
                        
                        p += 1
        
    def string(self):
        return "%s(%sx%sx%s, max_weight:%s) vol(%s) item_number(%s) filling_ratio(%s)" % (
            self.size, self.length, self.width, self.height, self.capacity,
            self.get_volume(), self.total_items, self.get_filling_ratio())

   
class Item:
    def __init__(self, name, length, width, height, weight, is_rotation_allowed, item_id):
        self.name = name
        self.length = length
        self.width = width
        self.height = height
        self.weight = weight
        self.is_rotation_allowed = is_rotation_allowed
        self.rotation_type = 0 # initial rotation type: (x, y, z) --> (l, w, h)
        self.position = START_POSITION # initial position: (0, 0, 0)
        self.number_of_decimals = DEFAULT_NUMBER_OF_DECIMALS
        self.item_id = item_id
    
    def format_numbers(self, number_of_decimals):
        self.length = set_to_decimal(self.length, number_of_decimals)
        self.height = set_to_decimal(self.height, number_of_decimals)
        self.width = set_to_decimal(self.width, number_of_decimals)
        self.weight = set_to_decimal(self.weight, number_of_decimals)
        self.number_of_decimals = number_of_decimals
    
    def get_volume(self):
        return set_to_decimal(
            self.length * self.height * self.width, self.number_of_decimals)
    
    def get_lenght_item(self):
        return self.length
    
    def get_width_item(self):
        return self.width
    
    def get_height_item(self):
        return self.height
     
    def get_total_weight(self):
        total_weight = 0
    
    def get_item_id(self):
        return self.item_id
    #
    def get_dimension(self): # 6 orientation types -- (x-axis, y-axis, z-axis)
        if self.rotation_type == RotationType.RT_LWH:
            dimension = [self.length, self.width, self.height]
        elif self.rotation_type == RotationType.RT_HLW:
            dimension = [self.height, self.length, self.width]
        elif self.rotation_type == RotationType.RT_HWL:
            dimension = [self.height, self.width, self.length]
        elif self.rotation_type == RotationType.RT_WHL:
            dimension = [self.width, self.height, self.length]
        elif self.rotation_type == RotationType.RT_WLH:
            dimension = [self.width, self.length, self.height]
        elif self.rotation_type == RotationType.RT_LHW:
            dimension = [self.length, self.height, self.width]
        else:
            dimension = []
        
        return dimension
    
    def get_position(self):
        return f"{self.position[0]} {self.position[1]} {self.position[2]}"
        
    def string(self):
        return "%s(%sx%sx%s, weight: %s) pos(%s) rt(%s) vol(%s)" % (
            self.name, self.length, self.width, self.height, self.weight,
            self.position, self.rotation_type, self.get_volume()
        )


class Packer:
    def __init__(self):
        self.bins = [] 
        self.unplaced_items = []
        self.placed_items = []
        self.unfit_items = []
        self.total_items = 0
        self.total_used_bins = 0 # not used yet
        self.used_bins = [] # not used yet
        self.utilization= 0.0
        
    def get_utilization(self):
        return self.utilization 
    
    def get_placed_items(self):
        return self.placed_items
    
    def add_bin(self, bin):
        return self.bins.append(bin)
    
    def add_item(self, item): 
        """Add unplaced items into bin's unplaced_items list.
        Args:
            item: an unplaced item.
        Returns:
            The unplaced item is added into bin's unplaced_items list."""
        
        self.total_items += 1
        return self.unplaced_items.append(item) 
    
    def pivot_dict(self, bin, item):
        """For each item to be placed into a certain bin, obtain a corresponding comparison parameter of each optional pivot that the item can be placed.
        Args:
            bin: a bin in bin list that a certain item will be placed into.
            item: an unplaced item in item list.
        Returns:
            a pivot_dict contain all optional pivot point and their comparison parameter of the item.
            a empty dict may be returned if the item couldn't be placed into the bin.
        """
        
        pivot_dict = {}
        can_put = False
        
        for axis in range(0, 3): 
            items_in_bin = bin.items 
            items_in_bin_temp = items_in_bin[:] 
            
            n = 0
            while n < len(items_in_bin):
                pivot = [0, 0, 0] 
                
                if axis == Axis.LENGTH: # axis = 0/ x-axis
                    ib = items_in_bin[n]
                    pivot = [ib.position[0] + ib.get_dimension()[0],
                            ib.position[1],
                            ib.position[2]]
                    try_put_item = bin.can_hold_item_with_rotation(item, pivot) 
                    
                    if try_put_item: 
                        can_put = True
                        q = 0
                        q = 0
                        ib_neigh_x_axis = []
                        ib_neigh_y_axis = []
                        ib_neigh_z_axis = []
                        right_neighbor = False
                        front_neighbor = False
                        above_neighbor = False
                        
                        while q < len(items_in_bin_temp):
                            if items_in_bin_temp[q] == items_in_bin[n]: 
                                q += 1 
                            
                            else:
                                ib_neighbor = items_in_bin_temp[q]
                                
                                if (
                                    ib_neighbor.position[0] > ib.position[0] + ib.get_dimension()[0] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] 
                                ): 
                                    right_neighbor = True
                                    x_distance = ib_neighbor.position[0] - (ib.position[0] + ib.get_dimension()[0])
                                    ib_neigh_x_axis.append(x_distance)
                                    
                                elif (
                                    ib_neighbor.position[1] >= ib.position[1] + ib.get_dimension()[1] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] + ib.get_dimension()[0] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] 
                                ):
                                    front_neighbor = True
                                    y_distance = ib_neighbor.position[1] - ib.position[1]
                                    ib_neigh_y_axis.append(y_distance)
                                
                                elif (
                                    ib_neighbor.position[2] >= ib.position[2] + ib.get_dimension()[2] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] + ib.get_dimension()[0] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] 
                                ):
                                    above_neighbor = True
                                    z_distance = ib_neighbor.position[2] - ib.position[2]
                                    ib_neigh_z_axis.append(z_distance)
                                
                                q += 1 
                                
                        if not right_neighbor: 
                            x_distance = bin.length - (ib.position[0] + ib.get_dimension()[0])
                            ib_neigh_x_axis.append(x_distance)
                        
                        if not front_neighbor: 
                            y_distance = bin.width - ib.position[1]
                            ib_neigh_y_axis.append(y_distance)
                        
                        if not above_neighbor: 
                            z_distance = bin.height - ib.position[2]
                            ib_neigh_z_axis.append(z_distance)
                        
                        distance_3D = [min(ib_neigh_x_axis), min(ib_neigh_y_axis), min(ib_neigh_z_axis)]
                        pivot_dict[tuple(pivot)] = distance_3D
                
                elif axis == Axis.WIDTH: # axis = 1/ y-axis
                    ib = items_in_bin[n]
                    pivot = [ib.position[0],
                            ib.position[1] + ib.get_dimension()[1],
                            ib.position[2]]
                    try_put_item = bin.can_hold_item_with_rotation(item, pivot) 
                    
                    if try_put_item: 
                        can_put = True
                        q = 0
                        ib_neigh_x_axis = []
                        ib_neigh_y_axis = []
                        ib_neigh_z_axis = []
                        right_neighbor = False
                        front_neighbor = False
                        above_neighbor = False
                        
                        while q < len(items_in_bin_temp):
                            if items_in_bin_temp[q] == items_in_bin[n]: 
                                q += 1 
                            
                            else:
                                ib_neighbor = items_in_bin_temp[q]
                                
                                if (
                                    ib_neighbor.position[0] >= ib.position[0] + ib.get_dimension()[0] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] + ib.get_dimension()[1] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] 
                                ):
                                    right_neighbor = True
                                    x_distance = ib_neighbor.position[0] - ib.position[0]
                                    ib_neigh_x_axis.append(x_distance)
                                
                                elif (
                                    ib_neighbor.position[1] > ib.position[1] + ib.get_dimension()[1] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] 
                                ):
                                    front_neighbor = True
                                    y_distance = ib_neighbor.position[1] - (ib.position[1] + ib.get_dimension()[1])
                                    ib_neigh_y_axis.append(y_distance)
                                
                                elif (
                                    ib_neighbor.position[2] >= ib.position[2] + ib.get_dimension()[2] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] + ib.get_dimension()[1] 
                                ):
                                    above_neighbor = True
                                    z_distance = ib_neighbor.position[2] - ib.position[2]
                                    ib_neigh_z_axis.append(z_distance)
                                
                                q += 1
                        
                        if not right_neighbor: 
                            x_distance = bin.length - ib.position[0]
                            ib_neigh_x_axis.append(x_distance)
                        
                        if not front_neighbor: 
                            y_distance = bin.width - (ib.position[1] + ib.get_dimension()[1])
                            ib_neigh_y_axis.append(y_distance)
                        
                        if not above_neighbor: 
                            z_distance = bin.height - ib.position[2]
                            ib_neigh_z_axis.append(z_distance)
                        
                        distance_3D = [min(ib_neigh_x_axis), min(ib_neigh_y_axis), min(ib_neigh_z_axis)]
                        pivot_dict[tuple(pivot)] = distance_3D
            
                elif axis == Axis.HEIGHT: # axis = 2/ z-axis
                    ib = items_in_bin[n]
                    pivot = [ib.position[0],
                            ib.position[1],
                            ib.position[2] + ib.get_dimension()[2]]
                    try_put_item = bin.can_hold_item_with_rotation(item, pivot) 
                    
                    if try_put_item: 
                        can_put = True
                        q = 0
                        ib_neigh_x_axis = []
                        ib_neigh_y_axis = []
                        ib_neigh_z_axis = []
                        right_neighbor = False
                        front_neighbor = False
                        above_neighbor = False
                        
                        while q < len(items_in_bin_temp):
                            if items_in_bin_temp[q] == items_in_bin[n]: 
                                q += 1 
                            
                            else:
                                ib_neighbor = items_in_bin_temp[q]
                                
                                if (
                                    ib_neighbor.position[0] >= ib.position[0] + ib.get_dimension()[0] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] + ib.get_dimension()[2] 
                                ):
                                    right_neighbor = True
                                    x_distance = ib_neighbor.position[0] - ib.position[0]
                                    ib_neigh_x_axis.append(x_distance)
                                
                                elif (
                                    ib_neighbor.position[1] > ib.position[1] + ib.get_dimension()[1] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] and 
                                    ib_neighbor.position[2] + ib_neighbor.get_dimension()[2] > ib.position[2] + ib.get_dimension()[2] 
                                ):
                                    front_neighbor = True
                                    y_distance = ib_neighbor.position[1] - (ib.position[1] + ib.get_dimension()[1])
                                    ib_neigh_y_axis.append(y_distance)
                                
                                elif (
                                    ib_neighbor.position[2] >= ib.position[2] + ib.get_dimension()[2] and 
                                    ib_neighbor.position[1] + ib_neighbor.get_dimension()[1] > ib.position[1] and 
                                    ib_neighbor.position[0] + ib_neighbor.get_dimension()[0] > ib.position[0] 
                                ):
                                    above_neighbor = True
                                    z_distance = ib_neighbor.position[2] - ib.position[2]
                                    ib_neigh_z_axis.append(z_distance)
                                
                                q += 1
                                
                        if not right_neighbor: 
                            x_distance = bin.length - ib.position[0]
                            ib_neigh_x_axis.append(x_distance)
                        
                        if not front_neighbor: 
                            y_distance = bin.width - ib.position[1]
                            ib_neigh_y_axis.append(y_distance)
                        
                        if not above_neighbor: 
                            z_distance = bin.height - (ib.position[2] + ib.get_dimension()[2])
                            ib_neigh_z_axis.append(z_distance)
                        
                        distance_3D = [min(ib_neigh_x_axis), min(ib_neigh_y_axis), min(ib_neigh_z_axis)]
                        pivot_dict[tuple(pivot)] = distance_3D
                
                n += 1
        
        return pivot_dict
    
    def pivot_list(self, bin, item):
        """Obtain all optional pivot points that one item could be placed into a certain bin.
        Args:
            bin: a bin in bin list that a certain item will be placed into.
            item: an unplaced item in item list.
        Returns:
            a pivot_list containing all optional pivot points that the item could be placed into a certain bin.
        """
        
        pivot_list = [] 
        
        for axis in range(0, 3): 
            items_in_bin = bin.items 
            
            for ib in items_in_bin: 
                pivot = [0, 0, 0] 
                if axis == Axis.LENGTH: # axis = 0/ x-axis
                    pivot = [ib.position[0] + ib.get_dimension()[0],
                            ib.position[1],
                            ib.position[2]]
                elif axis == Axis.WIDTH: # axis = 1/ y-axis
                    pivot = [ib.position[0],
                            ib.position[1] + ib.get_dimension()[1],
                            ib.position[2]]
                elif axis == Axis.HEIGHT: # axis = 2/ z-axis
                    pivot = [ib.position[0],
                            ib.position[1],
                            ib.position[2] + ib.get_dimension()[2]]
        
                pivot_list.append(pivot)
            
        return pivot_list 
    
    def choose_pivot_point(self, bin, item):
        """Choose the optimal one from all optional pivot points of the item after comparison.
        Args:
            bin: a bin in bin list that a certain item will be placed into.
            item: an unplaced item in item list.
        Returns:
            the optimal pivot point that a item could be placed into a bin.
        """
        
        can_put = False
        pivot_available = []
        pivot_available_temp = []
        vertex_3d = []
        vertex_2d = []
        vertex_1d = []
        
        n = 0
        m = 0
        p = 0
        
        pivot_list = self.pivot_list(bin, item)
        
        for pivot in pivot_list:
            try_put_item = bin.can_hold_item_with_rotation(item, pivot)
            
            if try_put_item:
                can_put = True
                pivot_available.append(pivot)
                pivot_temp = sorted(pivot)
                pivot_available_temp.append(pivot_temp)
        
        if pivot_available:
            while p < len(pivot_available_temp):
                vertex_3d.append(pivot_available_temp[p][0])
                p += 1
            
            p = 0
            while p < len(pivot_available_temp): 
                if pivot_available_temp[p][0] == min(vertex_3d):
                    n += 1
                    vertex_2d.append(pivot_available_temp[p][1])
                
                p += 1
        
            if n == 1:
                p = 0
                while p < len(pivot_available_temp):
                    if pivot_available_temp[p][0] == min(pivot_available_temp[p]):
                        return pivot_available[p]
                
                    p += 1
        
            else:
                p = 0
                while p < len(pivot_available_temp):
                    if (
                        pivot_available_temp[p][0] == min(pivot_available_temp[p]) and 
                        pivot_available_temp[p][1] == min(vertex_2d)
                    ):
                        m += 1
                        vertex_1d.append(pivot_available_temp[p][2])
                
                    p += 1
        
            if m == 1:
                p = 0
                while p < len(pivot_available_temp):
                    if (
                        pivot_available_temp[p][0] == min(pivot_available_temp[p]) and 
                        pivot_available_temp[p][1] == min(vertex_2d)
                    ):
                        return pivot_available[p]
                
                    p += 1
        
            else:
                p = 0
                while p < len(pivot_available_temp):
                    if (
                        pivot_available_temp[p][0] == min(pivot_available_temp[p]) and
                        pivot_available_temp[p][1] == min(vertex_2d) and
                        pivot_available_temp[p][2] == min(vertex_1d)
                    ):
                        return pivot_available[p]
                
                    p += 1
        
        if not pivot_available:
            return can_put
        
    def pack_to_bin(self, bin, item): 
        """For each item and each bin, perform whole pack process with optimal orientation and pivot point.
        Args:
            bin: a bin in bin list that a certain item will be placed into.
            item: an unplaced item in item list.
        Returns: return value is void.
        """
        
        if not bin.items:
            response = bin.put_item(item, START_POSITION, [bin.length, bin.width, bin.height])
            
            if not response:
                bin.unfitted_items.append(item)
            
            return 
        
        else:
            pivot_point = self.choose_pivot_point(bin, item)
            pivot_dict = self.pivot_dict(bin, item)
                
            if not pivot_point:
                bin.unfitted_items.append(item)
                return 
                
            distance_3D = pivot_dict[tuple(pivot_point)]
            response = bin.put_item(item, pivot_point, distance_3D)
            return  
            
    def pack(
        self, bigger_first=True, number_of_decimals=DEFAULT_NUMBER_OF_DECIMALS):
        """For a list of items and a list of bins, perform the whole pack process.
        Args:
            bin: a bin in bin list that a certain item will be placed into.
            item: an unplaced item in item list.
        Returns:
            For each bin, print detailed information about placed and unplaced items.
            Then, print the optimal bin with highest packing rate.
        """
        
        for bin in self.bins:
            bin.format_numbers(number_of_decimals)
            
        for unplaced_item in self.unplaced_items:
            unplaced_item.format_numbers(number_of_decimals)
        
        self.bins.sort(
            key = lambda bin: bin.get_volume()) # default order of bins: from smallest to biggest
        self.unplaced_items.sort(
            key = lambda unplaced_item: unplaced_item.get_volume(), reverse=bigger_first) # default order of items: from biggest to smallest
        
        filling_ratio_list = []
        
        for bin in self.bins: 
            for unplaced_item in self.unplaced_items: 
                bin.unplaced_items.append(unplaced_item) 
        
        for bin in self.bins:
            for unplaced_item in self.unplaced_items:
                self.pack_to_bin(bin, unplaced_item)
                
            print(bin.string())
            print("SI??AN ??TEMLER:")
            for item in bin.items:
                print(item.string())
            
            print("\nSI??MAYAN ??TEMLER:")
            for item in bin.unfitted_items:
                print(item.string())
            
            filling_ratio_list.append(bin.get_filling_ratio())
            
        max_filling_ratio = max(filling_ratio_list)
        self.utilization = max_filling_ratio
        
        for bin in self.bins:
            if bin.get_filling_ratio() == max_filling_ratio: 
                for item in bin.items:
                    self.placed_items.append(item)
                print("\nEn y??ksek doldurma oran??na sahip se??ili kutu: ", bin.string())

class Order_Info:
    def __init__(self, order_id, country_name, item_types):
        self.order_id = order_id
        self.country_name = country_name
        self.item_types =  item_types
     
    def get_order_id(self):
        return self.order_id
    
    def get_country_name(self):
        return self.country_name
    
class Container_Info:
    def __init__(self,container_name, x,y,z):
        self.container_name= container_name
        self.x = x
        self.y = y
        self.z = z

    def get_container_name(self):
        return self.container_name
    
    def get_x(self):
        return self.x
    
    def get_y(self):
        return self.y
    
    def get_z(self):
        return self.z    

class Box_Info:
    def __init__(self, i_d, amount, x, y, z, rotation_type):
        self.i_d = i_d
        self.amount = amount
        self.x = x
        self.y = y
        self.z = z
        self.rotation_type = rotation_type
       
    def get_amount(self):
        return self.amount
    
    def get_i_d(self):
        return self.i_d
    
    def get_x(self):
        return self.x
    
    def get_y(self):
        return self.y
    
    def get_z(self):
        return self.z
    
    def get_rotation_type(self):
        return self.rotation_type

# args from system call
path = sys.argv[1]
file_name = sys.argv[2]

with open(path + "/" + file_name, 'r') as f:
    lines = f.readlines()

f.close()  

order = []
containers = []
boxes_ordered = []

for line in lines:
    values = line.split()
    if len(values) == 3:
        # This is the first line with order_id, country_name, and pisink
        order_id = values[0]
        country_name = values[1]
        item_types = values[2]
        order.append(Order_Info(order_id, country_name, item_types))
        
    elif len(values) == 4:
        # This is the second line with container name and dimensions
        container_name = values[0]
        x = values[1]
        y = values[2]
        z = values[3]
        containers.append(Container_Info(container_name, x, y, z))
        
    else:
        # This is the third line with box id, amount, dimensions, and rotation type
        i_d = values[0]
        amount = values[1]
        x = values[2]
        y = values[3]
        z = values[4]
        rotation_type = values[5]
        boxes_ordered.append(Box_Info(i_d, amount, x, y, z, rotation_type))
        
        
        
packer = Packer()
container_1 = Bin(containers[0].get_container_name(),containers[0].get_x(),containers[0].get_y(),containers[0].get_z(),9999)
packer.add_bin(container_1)
for item in boxes_ordered:
    print(item.get_amount())
    for i in range(int(item.get_amount())):
        packer.add_item(Item('item',item.get_x(),item.get_y(), item.get_z(), 1 , item.get_rotation_type(),item.get_i_d()))
        

    
packer.pack()   

with open(path + '/genetic_output.txt', 'w') as file:
    file.write(f"{order[0].get_order_id()} {order[0].get_country_name()} {container_1.get_size()} {packer.get_utilization()}")
    file.write(f"\n{container_1.get_lenght_bin()} {container_1.get_width_bin()} {container_1.get_height_bin()}")
  
    for item in packer.get_placed_items():
        file.write(f"\n{item.get_position()} {item.get_lenght_item()} {item.get_width_item()} {item.get_height_item()} {item.get_item_id()}  ")

file.close()        
