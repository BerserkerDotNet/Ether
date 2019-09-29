window.BlazorComponents = window.BlazorComponents || {};
window.BlazorComponents.DataTableInterop = {
    initializeDataTable: function (table) {
        $(table).DataTable({
            'paging': true,
            'lengthChange': true,
            'searching': true,
            'ordering': false,
            'info': true,
            'autoWidth': true
        });
    }
};
window.BlazorComponents.Utils = {
    getAllSelectedOptions: function (select) {
        return $(select).val();
    },
    failValidation: function (id) {
        $('#' + id).find(':input').addClass('is-invalid', 'was-validated');
    },
    succeedValidation: function (id) {
        $('#' + id).find(':input').removeClass('is-invalid', 'was-validated');
    },
    buttonState: function (btn, state) {
        var button = $(btn);
        if (state === 'loading') {
            var loadingText = button.data('loading-text');
            button.html(loadingText);
            button.attr('disabled', true);
        } else {
            var loadingText = button.data('normal-text');
            button.html(loadingText);
            button.removeAttr('disabled');
        }
    },
    print: function () {
        window.print();
    },
    saveAsFile: function (filename, bytesBase64) {
        var link = document.createElement('a');
        link.download = filename;
        link.href = "data:application/octet-stream;base64," + bytesBase64;
        document.body.appendChild(link); // Needed for Firefox
        link.click();
        document.body.removeChild(link);
    },
    getValue: function (element) {
        return $(element).val();
    },
    setValue: function (element, value) {
        return $(element).val(value);
    }
};

window.BlazorComponents.Notify = {
    notify: function (type, title, message, timer) {
        $.notify({
            title: title,
            message: message
        },
            {
                type: type,
                timer: timer,
                placement: {
                    from: "top",
                    align: "right"
                }
            });
    }
}

window.BlazorComponents.DateRangePicker = {
    init: function (element, blazorComponent) {
        var start = moment().subtract(6, 'days').format('MM/DD/YYYY');
        var end = moment().format('MM/DD/YYYY');
        $(element).daterangepicker({
            "autoApply": true,
            ranges: {
                'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                'Last Quater': [moment().subtract(3, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
            },
            "alwaysShowCalendars": true,
            "autoUpdateInput": true,
            "startDate": start,
            "endDate": end,
            "minDate": moment().subtract(6, 'month'),
            "maxDate": moment().add(10, 'days'),
            "opens": "right"
        })
        .on('apply.daterangepicker', function (ev, picker) {
            blazorComponent.invokeMethod("OnRangeChanged", picker.startDate.format('YYYY-MM-DD'), picker.endDate.format('YYYY-MM-DD'))
            });
        blazorComponent.invokeMethod("OnRangeChanged", start, end);
    },

    getStartDate: function (element) {
        return $(element).data('daterangepicker').startDate.format('MM/DD/YYYY');
    },

    getEndDate: function (element) {
        return $(element).data('daterangepicker').endDate.format('MM/DD/YYYY');
    }
};

window.BlazorComponents.SummerNoteBootstrap = {
    init: function (element, value, blazorComponent) {
        function getButtons() {
            return {
                Profile: createButton("Profile", "{Profile}"),
                Points: createButton("Points", "{Points}"),
                WorkItems: createButton("Work Items", "{WorkItems}")
            }
        };

        function createButton(text, content) {
            return function (context) {
                var ui = $.summernote.ui;

                var button = ui.button({
                    contents: '<i class="fa fa-child"/> ' + text,
                    tooltip: text,
                    click: function () {
                        context.invoke('editor.insertText', content);
                    }
                });

                return button.render();
            }
        };

        var buttons = getButtons();
        $(element).summernote({
            placeholder: '',
            tabsize: 1,
            height: 500,
            callbacks: {
                onChange: function (contents, $editable) {
                    blazorComponent.invokeMethod("OnChanged", contents);
                }
            },
            toolbar: [
                ['style', ['bold', 'italic', 'underline', 'clear']],
                ['font', ['strikethrough', 'superscript', 'subscript']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['misc', ['undo', 'redo', 'fullscreen', 'codeview']],
            ],

            buttons: buttons
        });
        $(element).summernote('code', value);
        $('#summernote').summernote('fontSize', 11);
        $('#summernote').summernote('fontName', 'Calibri (Body)');
    }
};