ko.collapse = {};
ko.collapse.expand = function (containerSelector, element) {
    $(containerSelector).slideDown();
    $(element).html("<i class='fa fa-minus'></i>");
};
ko.collapse.collapse = function (containerSelector, element) {
    $(containerSelector).slideUp();
    $(element).html("<i class='fa fa-plus'></i>");
};

ko.bindingHandlers.collapse = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var allBindings = allBindingsAccessor();
        var collapseBindings = allBindings.collapse;

        var contentSelector = ko.unwrap(collapseBindings.content);
        var isCollapsed = collapseBindings.isCollapsed;
        var options = ko.unwrap(collapseBindings.options) || {};

        if (!contentSelector) {
            console.log("Unable to setup collapse due to no content being set");
            return;
        }

        if (!isCollapsed) { isCollapsed = ko.observable(false); }
        if (!ko.isObservable(isCollapsed)) { isCollapsed = ko.observable(isCollapsed); }

        var checkValueChange = function (isCollapsed) {
            if (isCollapsed) {
                ko.collapse.collapse(contentSelector, element);
            }
            else {
                ko.collapse.expand(contentSelector, element);
            }
        };

        isCollapsed.subscribe(checkValueChange);

        var checkStyleChange = function (isCollapsed) {
            if (isCollapsed) {
                $(element).removeClass(options.expandedClass);
                $(element).addClass(options.collapsedClass)
            }
            else {
                $(element).addClass(options.expandedClass);
                $(element).removeClass(options.collapsedClass);
            }
        };

        if (options.expandedClass || options.collapsedClass)
        { isCollapsed.subscribe(checkStyleChange); }

        $(element).click(function () {
            isCollapsed(!isCollapsed());
        });

        isCollapsed.valueHasMutated();
    }
};


ko.bindingHandlers.trimLengthText = {};
ko.bindingHandlers.trimText = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var trimmedText = ko.computed(function () {
            var untrimmedText = ko.utils.unwrapObservable(valueAccessor());
            if (untrimmedText == null) return untrimmedText;
            var defaultMaxLength = 30;
            var minLength = 5;
            var maxLength = ko.utils.unwrapObservable(allBindingsAccessor().trimTextLength) || defaultMaxLength;
            if (maxLength < minLength) maxLength = minLength;
            var text = untrimmedText.length > maxLength ? untrimmedText.substring(0, maxLength - 1) + '...' : untrimmedText;
            return text;
        });
        ko.applyBindingsToNode(element, {
            text: trimmedText
        }, viewModel);

        return {
            controlsDescendantBindings: true
        };
    }
};

ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var $el = $(element);

        //initialize datepicker with some optional options
        var options = allBindingsAccessor().datepickerOptions || {};
        $el.datepicker(options);

        //handle the field changing
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            var date = $el.datepicker("getDate");

            if (!isNaN(date))
                observable(date);
        });

        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $el.datepicker("destroy");
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());

        if (!isNaN(value))
            $(element).datepicker("setDate", value);
    }
}

ko.bindingHandlers.numericValue = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var underlyingObservable = valueAccessor();
        var interceptor = ko.dependentObservable({
            read: underlyingObservable,
            write: function (value) {
                if (!isNaN(value)) {
                    underlyingObservable(parseFloat(value));
                }
            }
        });
        ko.bindingHandlers.value.init(element, function () { return interceptor }, allBindingsAccessor);
    },
    update: ko.bindingHandlers.value.update
};

ko.bindingHandlers.clickAndStop = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, context) {
        var handler = ko.utils.unwrapObservable(valueAccessor()),
            newValueAccessor = function () {
                return function (data, event) {
                    handler.call(viewModel, data, event);
                    event.cancelBubble = true;
                    if (event.stopPropagation) event.stopPropagation();
                };
            };

        ko.bindingHandlers.click.init(element, newValueAccessor, allBindingsAccessor, viewModel, context);
    }
};

ko.bindingHandlers.dateString = {
    update: function(element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor(),
        allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        var pattern = allBindings.datePattern || 'MM/DD/YYYY';
        $(element).text(moment(valueUnwrapped).format(pattern));
    }
}

ko.bindingHandlers.timeString = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor(),
        allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        var pattern = allBindings.timePattern || 'HH:MM';
        $(element).text(moment(valueUnwrapped).format(pattern));
    }
}

/* Adds the binding dateValue to use with bootstra-datepicker
   Usage :
   <input type="text" data-bind="dateValue:birthday"/>
   <input type="text" data-bind="dateValue:birthday,format='MM/DD/YYY'"/>

 */
ko.bindingHandlers.dateValue = {

    init: function (element, valueAccessor, allBindings) {
        var format;
        var defaultFormat = 'yyyy/mm/dd'
        if (typeof allBindings == 'function') {
            format = allBindings().format || defaultFormat;
        }
        else
            format = allBindings.get('format') || defaultFormat;

        var dpicker = $(element).datepicker({
            'format': format
        }).on('changeDate', function (ev) {
            var newDate = moment(new Date(ev.date));
            var value = valueAccessor();
            var currentDate = moment(value() || new Date);
            newDate.hour(currentDate.hour());
            newDate.minute(currentDate.minute());
            newDate.second(currentDate.second());
            value(newDate.toDate());

        });
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var date = ko.unwrap(valueAccessor());
        if (date) {
            $(element).datepicker('setDate', date);
        }
    }
}

/* Adds the binding timeValue to use with bootstra-timepicker 
   This works with the http://jdewit.github.io/bootstrap-timepicker/index.html
   component.
   Use: use with an input, make sure to use your input with this format
   <div class="bootstrap-timepicker pull-right">
       <input id="timepicker3" type="text" class="input-small">
   </div>
*/
ko.bindingHandlers.timeValue = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var tpicker = $(element).timepicker();
        tpicker.on('changeTime.timepicker', function (e) {

            //Asignar la hora y los minutos
            var value = valueAccessor();
            if (!value) {
                throw new Error('timeValue binding observable not found');
            }
            var date = ko.unwrap(value);
            var mdate = moment(date || new Date());
            var hours24;
            if (e.time.meridian == "AM") {
                if (e.time.hours == 12)
                    hours24 = 0;
                else
                    hours24 = e.time.hours;
            }
            else {
                if (e.time.hours == 12) {
                    hours24 = 12;
                }
                else {
                    hours24 = e.time.hours + 12;
                }
            }

            mdate.hours(hours24)
            mdate.minutes(e.time.minutes);
            $(element).data('updating', true);
            value(mdate.toDate());
            $(element).data('updating', false);


        })
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        //Avoid recursive calls
        if ($(element).data('updating')) {
            return;
        }
        var date = ko.unwrap(valueAccessor());

        if (date) {
            var time = moment(date).format("hh:mm a");
            $(element).timepicker('setTime', time);
        }
    }
}

// https://gist.github.com/jccounihan
ko.bindingHandlers.iCheckBox = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $el = $(element);
        var observable = valueAccessor();

        $el.iCheck({
            checkboxClass: 'icheckbox_minimal-blue',
            inheritClass: true
        });
        
        var enableTrigger = allBindingsAccessor().enableTrigger;
        // ifChecked handles tabs and clicks
        $el.on('ifChecked', function (e) {
            observable(true);            
            if (enableTrigger != undefined && enableTrigger) 
                $(element).parent().trigger("click");
        });
        $el.on('ifUnchecked', function (e) {
            observable(false);
            if (enableTrigger != undefined && enableTrigger)
                $(element).parent().trigger("click");
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This update handles both the reverting of values from cancelling edits, and the initial value setting.
        var $el = $(element);
        var value = ko.unwrap(valueAccessor());
        if (value == true) {
            $el.iCheck('check');
        } else if (value == false || value == null || value == "") { // Handle clearing the value on reverts.
            $el.iCheck('uncheck');
        }
    }
};

ko.bindingHandlers.iRadio = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        $(element).iCheck({
            radioClass: 'iradio_minimal-blue',
            increaseArea: "-10%",
        });
        $(element).on('ifChanged', function () {
            var observable = valueAccessor();
            observable($(element).val());
        });        

        var enableTrigger = allBindingsAccessor().enableTrigger;
        if (enableTrigger != undefined && enableTrigger) {
            $(element).on('ifChecked', function () {
                $(element).parent().trigger("click");
            });
        }
    },
    update: function (element, valueAccessor) {
        var observable = valueAccessor();        
        if (observable()) 
            $(element).iCheck('check');
    }
};

// https://github.com/select2/select2/wiki/Knockout.js-Integration
ko.bindingHandlers.select2 = {
    init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
        ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
            $(el).select2('destroy');
        });

        var allBindings = allBindingsAccessor(),
            select2 = ko.utils.unwrapObservable(allBindings.select2);

        $(el).select2(select2);
    },
    update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
        var allBindings = allBindingsAccessor();

        if ("value" in allBindings) {
            if (allBindings.select2.multiple && allBindings.value().constructor != Array) {
                $(el).select2("val", allBindings.value().split(","));
            }
            else {
                $(el).select2("val", allBindings.value());
            }
        } else if ("selectedOptions" in allBindings) {
            var converted = [];
            var textAccessor = function (value) { return value; };
            if ("optionsText" in allBindings) {
                textAccessor = function (value) {
                    var valueAccessor = function (item) { return item; }
                    if ("optionsValue" in allBindings) {
                        valueAccessor = function (item) { return item[allBindings.optionsValue]; }
                    }
                    var items = $.grep(allBindings.options(), function (e) { return valueAccessor(e) == value });
                    if (items.length == 0 || items.length > 1) {
                        return "UNKNOWN";
                    }
                    return items[0][allBindings.optionsText];
                }
            }
            $.each(allBindings.selectedOptions(), function (key, value) {
                converted.push({ id: value, text: textAccessor(value) });
            });
            $(el).select2("data", converted);
        }
    }
};

// https://github.com/faulknercs/Knockstrap
ko.bindingHandlers.alert = {
    init: function () {
        return { controlsDescendantBindings: true };
    },

    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element),
            value = valueAccessor(),
            usedTemplateEngine = !value.template ? ko.stringTemplateEngine.instance : null,
            userTemplate = ko.unwrap(value.template) || 'alertInner',
            template, data;

        // for compatibility with ie8, use '1' and '8' values for node types
        if (element.nodeType === (Node.ELEMENT_NODE || 1)) {
            template = userTemplate;
            data = value.data || { message: value.message };

            $element.addClass('alert fade in').addClass('alert-' + (ko.unwrap(value.type)));
        } else if (element.nodeType === (Node.COMMENT_NODE || 8)) {
            template = 'alert';
            data = {
                innerTemplate: {
                    name: userTemplate,
                    data: value.data || { message: value.message },
                    templateEngine: usedTemplateEngine
                },
                type: 'alert-' + (ko.unwrap(value.type))
            };

        } else {
            throw new Error('alert binding should be used with dom elements or ko virtual elements');
        }

        ko.renderTemplate(template, bindingContext.createChildContext(data), ko.utils.extend({ templateEngine: usedTemplateEngine }, value.templateOptions), element);
    }
};
ko.virtualElements.allowedBindings.alert = true;

ko.unwrap = ko.unwrap || ko.utils.unwrapObservable;