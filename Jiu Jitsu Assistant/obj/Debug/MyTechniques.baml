   M S B A M L   `   `   `  ����N  JJiu Jitsu Assistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:Jiu_Jitsu_AssistantJiu_Jitsu_Assistant  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   	   &     Jiu_Jitsu_Assistant.MyTechniques   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:Jiu_Jitsu_Assistant     5   	   Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4:
extToolkit)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   0sys&clr-namespace:System;assembly=mscorlib  5   	   -   5        $��window1��5   	      MyTechniques_Title  � Title#  �   5
   	   $��800px��5   	   $��1300px��6   #��� ��5   	    � 
ResizeMode$ NoResize=�5   	    � Icon$ /images/bjj-logo.png��5   	   .��5      � -   5         �Name$ mainGrid��5   
   $��Stretch=�6   $��Stretch=�68    J� IsHitTestVisible . $��-5,0,5,0q�6n    � ColumnDefinitions 5      �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5      �� $��523,33,514,481q�6   $��images/bjj-logo.png��6&   $��3��6C   &   System.Windows.Controls.DataGrid  5      -   5          Name$ techniquesGrid��5        RowStyleWithAlternation   RowStyle# [ 6)    �� AlternationCount$ 2��6]   $
��Left=�6r   $��
68,306,0,0q�6�   $	��Top=�6�   	   AutoGenerateColumns	 .  $	��256��5   )   "
   VerticalScrollBarVisibility$
 Visible=�66   $��0��6\      CanUserAddRows .  $	��643��6�      SelectionMode$ Single=�5   :   $��3��6}   ��5      ��    Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Technique name��6!      Width5  /System.Windows.Controls.DataGridLengthConverter$	 200 6R   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *name+6R     5       $ Learned��6!   $ 70 6r    62   �� *date_learned+ �� StringFormat$ {0:dd.MM.yyyy}��6r     5!      $
 From��6!   $	 125 6Q    6/   �� *position_from+6Q     5"      $ To��6!   $ * 6M    6-   �� *position_to+6M     5#      $
 Belt��6!      MaxWidth$ 0Q�6/   $ * 6[    6<   �� *
belt_level+6[     5$      $ 0Q�6!   $ Id��6.   $ * 6[    6:   �� *technique_id+6[   5&   	     5'      -   5        $ techniqueGroupsGrid��5'      # [ 6.   $ 2��6b   $
��Left=�6w   $	��Top=�6�   	 .   J� RenderTransformOrigin$ 2.386,4.962&�5)      $��	68,89,0,0q�65    .  ��5(   Q   ��  5*        5+      $ Group��6!    60   �� *name+5-   	   �� 5/      -   5         ��Name$ setups_textblock��5/      
�������$
��Left=�6?   $��129,591,0,0q�6Z   $
��Wrap=�6o   $	��Top=�6�   $	��138��6�   $	��582��6�   $��3��6�   �� 50      -   5         ��Name$ setups_label��50      $��Setups:��6$   $
��Left=�66   $��
64,586,0,0q�6Q   $	��Top=�6e   $ -0.684,-0.385&�6}   �� 55      -   5        $ label��55        AddTechnique_Header_tech#���  6   $��2��6R   $
��Left=�6b   $��
216,14,0,0q�6}   $	��Top=�6�   $��24(�6�   $ 
0.545,0.19&�6�   �� 56      -   5        $ 
label_Copy��56      $��0��6     MyTechniques_Header_tech#���  62   $
��Left=�6g   $��
293,14,0,0q�6�   $	��Top=�6�   $��24(�6�   $ 
0.545,0.19&�6�   �� 57      $
��Left=�6   $	��403��6*   $	��Top=�67   $	��492��6O   $��2��6[   $��
107,57,0,0q�6k   .+�58      �� -	   5        $	 lbl��58        NameLabel_tech#���  6   $
��Left=�6I   $	��Top=�6d   $ 0.211,-0.154&�6|    �� Left$ 48��6�   
 �� Top$ 30��6�   $��14(�6�   (  "Xceed.Wpf.Toolkit.WatermarkTextBox  59      -
   5        $��techniqueName_textbox��59   '   $��25��6D   $	 171��6P    �� TextWrapping$
 Wrap=�6b   $ 34��6v   $	��235��6�      	Watermark$ Technique name��6�   6�   �� 5:      -   5        $ lbl_Copy��5:        DateLearned_tech#���  6#   $
��Left=�6P   $	��Top=�6k   $ 0.211,-0.154&�6�   $ 48��6�   $ 80��6�   $��14(�6�   (  "System.Windows.Controls.DatePicker  5;      -   5        $��dateLearned_datepicker��5;      $	 171��64   $ 83��6F   $	��235��6V   
 �� Now   SelectedDate# Z 6b   �� 5<      -   5        $ 	lbl_Copy1��5<        	Belt_tech#���  6$   $
��Left=�6J   $	��Top=�6e   $ 0.211,-0.154&�6}   $ 48��6�   $	 130��6�   $��14(�6�   �� 5=      -   5         ��Name$ belt_comboBox��5=      $��Center=�6+   $	 171��6M   $	 130��6_   $	��235��6p   $��29��6|   .@�5>      �� -   5        $��white��5>      
������� �� 
IsSelected . .��5?      ��  �� Orientation$ 
Horizontal=�6   .+�5@      �� $��/images/white-belt.jpg��6   $	��225��6:   $��15��6F   5A      5B      �� 5C      -   5        $
��blue��5C      .��5D      �� $ 
Horizontal=�6   .+�5E      �� $��/images/blue-belt.jpg��6   $	��225��69   $��15��6E   5F      5G      �� 5H      -   5        $��purple��5H      .��5I      �� $ 
Horizontal=�6   .+�5J      �� $��/images/purple-belt.jpg��6   $	��225��6;   $��15��6G   5K      5L      �� 5M      -   5        $��brown��5M      .��5N      �� $ 
Horizontal=�6   .+�5O      �� $��/images/brown-belt.jpg��6   $	��225��6:   $��15��6F   5P      5Q      �� 5R      -   5        $��black��5R      .��5S      �� $ 
Horizontal=�6   .+�5T      �� $��/images/black-belt.jpg��6   $	��225��6:   $��15��6F   5U      5V      5W      �� 5X      -   5        $ label1��5X        TechniqueGroup_tech#���  6!   $ 48��6Q   $	 180��6b   $��14(�6s   �� 5Y      -   5        $ techniqueGroup_comboBox��5Y      $��Center=�65   $	 171��6P   $	 180��6b   $	��235��6s   $��29��6   $��Center=�6�   �� 5Z      -   5         ��Name$ addTechnique_button��5Z        AddTechniqueButton_tech#���  6/   $
��Left=�6c   $	��Top=�6~   $	��100��6�   $ 0.507,0.773&�6�   $	 306��6�   $	 368��6�   {�.  5[   <   �� 5\      -   5        $ 
Label_From��5\       	 FromPosition_tech#��� 	 6%   $ 48��6S   $	 230��6d   $ -0.079,-0.385&�6u   $��14(�6�   �� 5]      -   5        $ from_Position_comboBox��5]      $	 171��64   $	 230��6F   $	��235��6W   $��29��6c   $��Center=�6o   �� 5^      -   5        $ Label_To��5^       
 ToPosition_tech#��� 
 6#   $ 48��6O   $	 280��6`   $��14(�6q   �� 5_      -   5        $ to_Position_comboBox��5_      $	 171��62   $	 280��6D   $	��235��6U   $��29��6a   $��Center=�6m   �� 5`      -   5          ��Name$  nogi_checkBox��5`      $
��NoGi��6+   
�������$	 174��6M   $	 332��6_   ! d� 	IsChecked$
! TrueY�6p   5a   	   �� 5b      -   5        " ��Name$" setup_textBox��5b      $��2��6'   $
��Left=�67   $��71��6R   $��279,521,0,0q�6^   $
 Wrap=�6s   $	��Top=�5c      $	��240��6(   # �� AcceptsReturn# . {�.  6I   �� 5d      -   5        $ description_label��5d        DescriptionLabel_tech#���  6)   $��2��6[   $
��Left=�6k   $��158,517,0,0q�6�   $	��Top=�6�   $��14(�6�   �� 5e      -   5        $ add_setup_button��5e        AddSetupButton_tech#���  6)   $��2��6Y   $
��Left=�6i   $��419,609,0,0q�6�   $	��Top=�6�   $	��100��6�   {�.  6�   �� 5f      -   5        $ finish_technique_button��5f        FinishButton_tech#���  60   $��2��6^   $
��Left=�6n   $��158,688,0,0q�6�   $	��Top=�6�   $	��361��6�   6�   �� 5g      -    5        $ setup_label��5g        SetupLabel_tech#���  6#   $��2��6O   $
��Left=�6_   $��158,465,0,0q�6z   $	��Top=�6�   $ 1.184,1.615&�6�   $��18(�6�   5m      5n      