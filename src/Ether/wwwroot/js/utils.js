window.BlazorComponents = window.BlazorComponents || {};
window.BlazorComponents.Utils = {
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
    }
};

window.BlazorComponents.DateRangePicker = {
    init: function (element, blazorComponent) {
        var start = moment().subtract(6, 'days').format('MM/DD/YYYY');
        var end = moment().format('MM/DD/YYYY');
        console.log(moment());
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
            "maxDate": moment(),
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