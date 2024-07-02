

NumericSlider，结合文本框和滑动条，可快速设定数字；
NumericUpDown，解和文本框和两个按钮，上下，可快速设定数字；
PinchableGroupBox，可向上收起的groupbox，CheckPinchModes控制复选框变化后是否折叠面板，CheckEnableModes控制复选框变化后，内容的可用性；
BtnDropDown，点击按钮本身触发click，点击小按钮则触发dropDown（选中时），或pullUp（非选中时）；
NavigateBar，使用BtnDropDown的仿windows导航栏，交互模式下，用户通过点击按钮、下拉按钮进入新路径，或者在编辑模式下手动输入路径；
-用户操作后一般不会有太多受体，所以采用代理;
-内容变动全部为外部干预，用户操作单向输出到父级；
-使用pushNode和cutNode设置按钮串；
-左侧图标和文本地址需要单独使用Icon 和TextboxURL来设置，浏览历史需要调用AddHistory；
TextViewScroller，包含以下类
    RichTextBoxScroller，未测试，暂无使用的项目；
    TextBoxScroller，未测试
    WebBroswerScroller，未测试





附加内容
StaticResource，静态资源，供其他控件使用；



